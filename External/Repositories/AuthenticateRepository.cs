namespace Muonroi.BuildingBlock.External.Repositories
{
    public class AuthenticateRepository<TDbContext, TPermission>(
    MAuthenticateTokenHelper<TPermission> tokenHelper,
    TDbContext dbContext,
    MAuthenticateInfoContext authContext,
    MTokenInfo mTokenInfo) : MRepository<MUser>(dbContext, authContext), IAuthenticateRepository
        where TDbContext : MDbContext
        where TPermission : Enum
    {
        private readonly TDbContext _dbContext = dbContext;

        private readonly MAuthenticateInfoContext _authContext = authContext;

        public async Task<MResponse<LoginResponseModel>> Login(LoginRequestModel request, CancellationToken cancellationToken)
        {
            MResponse<LoginResponseModel> result = new();

            if (IsRequestInvalid(request, out string? errorMessage))
            {
                result.AddApiErrorMessage(errorMessage, [request.Username]);
                return result;
            }

            MUser? existedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == request.Username, cancellationToken: cancellationToken);

            if (existedUser is null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.InvalidCredentials), [request.Username]);
                return result;
            }

            MUserLoginAttempt? loginAttemptHistory = await _dbContext.MUserLoginAttempts
               .FirstOrDefaultAsync(x => x.UserGuid == existedUser.EntityId, cancellationToken);

            if (IsAccountLocked(loginAttemptHistory, out errorMessage))
            {
                result.AddApiErrorMessage(errorMessage, [request.Username]);
                return result;
            }

            if (loginAttemptHistory != null && loginAttemptHistory.LockTo <= Clock.Now)
            {
                existedUser.IsActive = true;
                await ResetLoginAttemptOnSuccess(existedUser, loginAttemptHistory, cancellationToken);
            }
            if (!MPasswordHelper.VerifyPassword(request.Password, existedUser.Password))
            {
                result.AddApiErrorMessage(nameof(SystemEnum.InvalidCredentials), [request.Username]);
                return result;
            }


            List<TPermission> permissions = await GetPermissionsOfUser(existedUser!.Id);

            GenerateAccessToken(existedUser, permissions, out string accessToken, out string tokenValidate);

            GenerateRefreshToken(out string refreshToken);

            result.Result = await GenerateLoginReply(accessToken, refreshToken, existedUser, tokenValidate);

            await ResetLoginAttemptOnSuccess(existedUser, loginAttemptHistory, cancellationToken);

            return result;
        }

        public async Task<MResponse<RefreshTokenResponseModel>> RefreshToken(RefreshTokenRequestModel request, CancellationToken cancellationToken)
        {
            MResponse<RefreshTokenResponseModel> result = new();

            MUser? existedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == _authContext.CurrentUserGuid, cancellationToken: cancellationToken);

            if (existedUser is null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.InvalidCredentials), [_authContext.CurrentUsername]);
                return result;
            }
            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(request.AccessToken, out string message);

            if (!string.IsNullOrEmpty(message))
            {
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result;
            }

            List<Claim> claims = principal.Claims.ToList();
            string? userIdentifier = claims.Find(c => c.Type == ClaimConstants.UserIdentifier)?.Value ?? string.Empty;
            Claim? tokenKey = claims.Find(c => c.Type == ClaimConstants.TokenValidityKey);
            string? tokenValidity = tokenKey?.Value ?? string.Empty;

            MRefreshToken? refresh = await _dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == request.RefreshToken
                                                        && x.TokenValidityKey == tokenValidity
                                                        && x.CreatorUserId == Guid.Parse(userIdentifier), cancellationToken: cancellationToken);

            if (refresh is null || refresh.Token != request.RefreshToken ||
                 refresh.IsDeleted || refresh.IsRevoked)
            {
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result;
            }
            if (refresh.ExpiredDate <= Clock.Now)
            {
                await RevokeRefreshToken(refresh, Guid.Parse(userIdentifier), "ExpireToken");
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result;
            }
            string tokenValidityKey = Guid.NewGuid().ToString();
            if (tokenKey is not null)
            {
                _ = claims.Remove(tokenKey);
                claims.Add(new(ClaimConstants.TokenValidityKey, tokenValidityKey));
            }
            List<TPermission> permissions = await GetPermissionsOfUser(existedUser!.Id);

            GenerateAccessToken(claims, out string newAccessToken);
            GenerateRefreshToken(out string newRefreshToken);
            await SaveRefreshToken(newRefreshToken, Guid.Parse(userIdentifier), tokenValidityKey);
            await RevokeRefreshToken(refresh, Guid.Parse(userIdentifier), "RefreshToken");
            await RemoveOldRefreshTokensByUser(Guid.Parse(userIdentifier));
            result.Result = new()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            return result;

        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, out string message)
        {
            message = string.Empty;
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,

                ValidIssuer = mTokenInfo.Issuer,
                ValidAudience = mTokenInfo.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(mTokenInfo.SigningKeys))
            };
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                return securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
                    ? throw new SecurityTokenException("Invalid token")
                    : principal;
            }
            catch (SecurityTokenException ex)
            {
                message = ex.Message;
                return new ClaimsPrincipal();
            }
        }
        private async Task RevokeRefreshToken(MRefreshToken token, Guid userId, string reason = "")
        {
            token.RevokedDate = Clock.Now;
            token.ReasonRevoked = reason;
            token.IsRevoked = true;
            token.LastModificationTime = Clock.Now;
            token.LastModificationUserId = userId;
            _ = _dbContext.Update(token);
            _ = await _dbContext.SaveChangesAsync();
        }

        private async Task<LoginResponseModel> GenerateLoginReply(string accessToken, string refreshToken, MUser existedUser, string tokenValidate)
        {

            LoginResponseModel loginReply = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Surname = existedUser.Surname,
                Name = existedUser.Name,
                EmailAddress = existedUser.EmailAddress,
                PhoneNumber = existedUser.PhoneNumber,
                IsPhoneNumberConfirmed = existedUser.IsPhoneNumberConfirmed,
                IsEmailConfirmed = existedUser.IsEmailConfirmed,
                IsActive = existedUser.IsActive,
            };

            await SaveRefreshToken(loginReply.RefreshToken, existedUser.EntityId, tokenValidate);

            await RemoveOldRefreshTokensByUser(existedUser.EntityId);

            return loginReply;
        }

        private async Task SaveRefreshToken(string refreshToken, Guid userId, string tokenValidityKey)
        {
            MRefreshToken token = new()
            {
                Token = refreshToken,
                TokenValidityKey = tokenValidityKey,
                ExpiredDate = Clock.Now.AddMinutes(mTokenInfo.RefreshTokenEIM),
                IsDeleted = false,
                IsRevoked = false,
                CreatorUserId = userId,
                CreationTime = Clock.Now
            };
            _ = await _dbContext.RefreshTokens.AddAsync(token);
            _ = await _dbContext.SaveChangesAsync();
        }
        private async Task RemoveOldRefreshTokensByUser(Guid userId)
        {
            List<MRefreshToken> tokensToDelete = [.. _dbContext.RefreshTokens
            .Where(rt =>
            (rt.IsDeleted || rt.IsRevoked ) &&
             rt.CreationTime.AddDays(mTokenInfo.RefreshTokenTTL) <= Clock.Now &&
                rt.CreatorUserId == userId)];

            if (tokensToDelete.Count != 0)
            {
                _dbContext.RefreshTokens.RemoveRange(tokensToDelete);
                _ = await _dbContext.SaveChangesAsync();
            }
        }

        private async Task ResetLoginAttemptOnSuccess(MUser existedUser, MUserLoginAttempt? loginAttemptHistory, CancellationToken cancellationToken)
        {
            if (loginAttemptHistory != null)
            {
                loginAttemptHistory.AttemptTime = 0;
                loginAttemptHistory.LockTo = DateTime.MinValue;
                _ = _dbContext.Update(loginAttemptHistory);
            }

            if (!existedUser.IsActive)
            {
                existedUser.IsActive = true;
                _ = _dbContext.Users.Update(existedUser);
            }

            _ = await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static bool IsRequestInvalid(LoginRequestModel request, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                errorMessage = nameof(SystemEnum.InvalidLoginInfo);
                return true;
            }
            return false;
        }

        private static bool IsAccountLocked(MUserLoginAttempt? loginAttemptHistory, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (loginAttemptHistory != null && loginAttemptHistory.LockTo > Clock.Now)
            {
                TimeSpan remainingLockTime = loginAttemptHistory.LockTo - Clock.Now;
                errorMessage = remainingLockTime.ToString();
                return true;
            }

            return false;
        }

        private void GenerateAccessToken(MUser user, List<TPermission> permissions, out string accessToken, out string tokenValidityKey)
        {
            tokenValidityKey = Guid.NewGuid().ToString();

            MUserModel userModel = new(user.EntityId.ToString(), user.UserName, tokenValidityKey);

            accessToken = tokenHelper.GenerateAuthenticateToken(userModel, permissions, Clock.Now.AddMinutes(mTokenInfo.ExpiryMinutes));
        }
        private static void GenerateRefreshToken(out string refreshToken)
        {
            byte[] randomNumber = new byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            refreshToken = Convert.ToBase64String(randomNumber);
        }

        private void GenerateAccessToken(List<Claim>? claims, out string refreshToken)
        {
            SymmetricSecurityKey securityKey = new(Convert.FromBase64String(mTokenInfo.SigningKeys));
            JwtSecurityToken token = new(
                mTokenInfo.Issuer,
                mTokenInfo.Audience,
                claims ?? [],
                DateTime.UtcNow,
                DateTime.UtcNow.Add(TimeSpan.FromMinutes(mTokenInfo.ExpiryMinutes)),
                new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );
            refreshToken = new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<List<TPermission>> GetPermissionsOfUser(long userId)
        {
            List<string> permissionNames = await (from user in _dbContext.Set<MUser>()
                                                  join userRole in _dbContext.Set<MUserRole>() on user.EntityId equals userRole.UserId
                                                  join role in _dbContext.Set<MRole>() on userRole.RoleId equals role.EntityId
                                                  join rolePermission in _dbContext.Set<MRolePermission>() on role.EntityId equals rolePermission.RoleId
                                                  join permission in _dbContext.Set<MPermission>() on rolePermission.PermissionId equals permission.EntityId
                                                  where user.Id == userId
                                                  select permission.Name).Distinct().ToListAsync();
            return permissionNames.Select(name => (TPermission)Enum.Parse(typeof(TPermission), name)).ToList();
        }
    }

}