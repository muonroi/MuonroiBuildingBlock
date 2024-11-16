

namespace Muonroi.BuildingBlock.Internal.Infrastructure.Authorize
{
    public static class AuthorizeInternal
    {
        internal static async Task ResolveTokenValidityKey<TDbContext, TPermission>(this TDbContext dbContext, string authorizationHeader,
           HttpContext context)
        where TDbContext : MDbContext
        where TPermission : Enum
        {
            List<Claim> claims = ExtractClaimsFromToken(authorizationHeader);

            string tokenValidity = GetClaimValue<string>(claims, ClaimConstants.TokenValidityKey) ?? string.Empty;

            MRefreshToken? refresh = await dbContext.RefreshTokens.SingleOrDefaultAsync(x =>
                                            x.TokenValidityKey == tokenValidity);

            if (refresh is null || refresh.IsDeleted || refresh.IsRevoked)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid token.");
                return;
            }
            context.Items.Add(nameof(MAuthenticateInfoContext.IsAuthenticated), true);
        }
        private static T? GetClaimValue<T>(List<Claim> claims, string claimType)
        {
            Claim? claim = claims.FirstOrDefault(c => c.Type == claimType);
            return claim != null && !string.IsNullOrEmpty(claim.Value) ? (T)Convert.ChangeType(claim.Value, typeof(T)) : default;
        }
        private static List<Claim> ExtractClaimsFromToken(string token)
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));
            return jwtToken.Claims.ToList();
        }

        internal static async Task<MResponse<LoginResponseModel>> ResolveLoginAsync<TDbContext, TPermission>(this TDbContext dbContext, LoginRequestModel request,
            MResponse<LoginResponseModel> result,
            MUser existedUser,
            MTokenInfo mTokenInfo,
            MAuthenticateTokenHelper<TPermission> tokenHelper,
            CancellationToken cancellationToken)
        where TDbContext : MDbContext
        where TPermission : Enum
        {
            MUserLoginAttempt? loginAttemptHistory = await dbContext.MUserLoginAttempts
                           .FirstOrDefaultAsync(x => x.UserGuid == existedUser.EntityId, cancellationToken);

            if (IsAccountLocked(loginAttemptHistory, out string errorMessage))
            {
                result.AddApiErrorMessage(errorMessage, [request.Username]);
                return result;
            }

            if (loginAttemptHistory != null && loginAttemptHistory.LockTo != DateTime.MinValue && loginAttemptHistory.LockTo <= Clock.UtcNow)
            {
                existedUser.IsActive = true;
                await ResetLoginAttemptOnSuccess(existedUser, loginAttemptHistory, dbContext, cancellationToken);
            }
            if (!MPasswordHelper.VerifyPassword(request.Password, existedUser.Password))
            {
                await HandleFailedLoginAttempt(existedUser, loginAttemptHistory, dbContext, cancellationToken);
                result.AddApiErrorMessage(nameof(SystemEnum.InvalidCredentials), [request.Username]);
                return result;
            }


            List<TPermission> permissions = await GetPermissionsOfUser<TDbContext, TPermission>(existedUser!.Id, dbContext);

            GenerateAccessToken(existedUser, permissions, out string accessToken, out string tokenValidate, mTokenInfo, tokenHelper);

            GenerateRefreshToken(out string refreshToken);

            result.Result = await GenerateLoginReply(accessToken, refreshToken, existedUser, tokenValidate, mTokenInfo, dbContext);

            await ResetLoginAttemptOnSuccess(existedUser, loginAttemptHistory, dbContext, cancellationToken);
            return result;
        }

        internal static async Task<MResponse<RefreshTokenResponseModel>> ResolveRefreshToken<TDbContext, TPermission>(
            this TDbContext dbContext,
            RefreshTokenRequestModel request,
            MResponse<RefreshTokenResponseModel> result,
             MUser existedUser,
            MTokenInfo mTokenInfo,
            CancellationToken cancellationToken)
            where TDbContext : MDbContext
            where TPermission : Enum
        {
            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(request.AccessToken, out string message, mTokenInfo);

            if (!string.IsNullOrEmpty(message))
            {
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result;
            }

            List<Claim> claims = principal.Claims.ToList();
            string? userIdentifier = claims.Find(c => c.Type == ClaimConstants.UserIdentifier)?.Value ?? string.Empty;
            Claim? tokenKey = claims.Find(c => c.Type == ClaimConstants.TokenValidityKey);
            string? tokenValidity = tokenKey?.Value ?? string.Empty;

            MRefreshToken? refresh = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == request.RefreshToken
                                                        && x.TokenValidityKey == tokenValidity
                                                        && x.CreatorUserId == Guid.Parse(userIdentifier), cancellationToken: cancellationToken);

            if (refresh is null || refresh.Token != request.RefreshToken ||
                 refresh.IsDeleted || refresh.IsRevoked)
            {
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result;
            }
            if (refresh.ExpiredDate <= Clock.UtcNow)
            {
                await RevokeRefreshToken(refresh, Guid.Parse(userIdentifier), dbContext, "ExpireToken");
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result;
            }
            string tokenValidityKey = Guid.NewGuid().ToString();
            if (tokenKey is not null)
            {
                _ = claims.Remove(tokenKey);
                claims.Add(new(ClaimConstants.TokenValidityKey, tokenValidityKey));
            }

            _ = await GetPermissionsOfUser<TDbContext, TPermission>(existedUser!.Id, dbContext);

            GenerateAccessToken(claims, out string newAccessToken, mTokenInfo);
            GenerateRefreshToken(out string newRefreshToken);
            await SaveRefreshToken(newRefreshToken, dbContext, Guid.Parse(userIdentifier), tokenValidityKey, mTokenInfo);
            await RevokeRefreshToken(refresh, Guid.Parse(userIdentifier), dbContext, "RefreshToken");
            await RemoveOldRefreshTokensByUser(Guid.Parse(userIdentifier), dbContext, mTokenInfo);
            result.Result = new()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            return result;
        }

        internal static async Task<MResponse<string>> ResolveTokenValidity<TDbContext>(this TDbContext dbContext, string tokenValidity, CancellationToken cancellationToken)
            where TDbContext : MDbContext
        {
            MResponse<string> result = new();
            if (string.IsNullOrEmpty(tokenValidity))
            {
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result;
            }
            MRefreshToken? refresh = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.TokenValidityKey == tokenValidity, cancellationToken: cancellationToken);
            if (refresh is null || refresh.IsDeleted || refresh.IsRevoked)
            {
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result;
            }
            result.Result = refresh.Token;
            return result;
        }


        private static async Task HandleFailedLoginAttempt<TDbContext>(MUser existedUser, MUserLoginAttempt? loginAttemptHistory, TDbContext dbContext,
            CancellationToken cancellationToken)
            where TDbContext : MDbContext
        {
            MUserLoginAttempt loginAttempt = loginAttemptHistory ?? new MUserLoginAttempt
            {
                UserGuid = existedUser.EntityId,
                CreationTime = Clock.UtcNow,
                AttemptTime = 0
            };

            loginAttempt.AttemptTime++;
            UpdateLoginAttemptStatus(existedUser, loginAttempt);

            _ = loginAttemptHistory == null
                ? await dbContext.MUserLoginAttempts.AddAsync(loginAttempt, cancellationToken)
                : dbContext.MUserLoginAttempts.Update(loginAttempt);

            _ = dbContext.Users.Update(existedUser);
            _ = await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static void UpdateLoginAttemptStatus(MUser existedUser, MUserLoginAttempt loginAttempt)
        {
            switch (loginAttempt.AttemptTime)
            {
                case 3:
                    loginAttempt.LockTo = Clock.UtcNow.AddMinutes(5);
                    break;
                case 4:
                    loginAttempt.LockTo = Clock.UtcNow.AddMinutes(10);
                    break;
                case 5:
                    loginAttempt.LockTo = Clock.UtcNow.AddMinutes(30);
                    break;
                case 6:
                    loginAttempt.LockTo = DateTime.MaxValue;
                    break;
            }

            if (loginAttempt.AttemptTime >= 3)
            {
                existedUser.IsActive = false;
            }
        }
        private static ClaimsPrincipal GetPrincipalFromExpiredToken(string token, out string message, MTokenInfo mTokenInfo)
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
        }//
        private static async Task RevokeRefreshToken<TDbContext>(MRefreshToken token, Guid userId, TDbContext dbContext, string reason = "")
            where TDbContext : MDbContext
        {
            token.RevokedDate = Clock.UtcNow;
            token.ReasonRevoked = reason;
            token.IsRevoked = true;
            token.LastModificationTime = Clock.UtcNow;
            token.LastModificationUserId = userId;
            _ = dbContext.Update(token);
            _ = await dbContext.SaveChangesAsync();
        }

        private static async Task<LoginResponseModel> GenerateLoginReply<TDbContext>(string accessToken,
            string refreshToken,
            MUser existedUser,
            string tokenValidate,
            MTokenInfo mTokenInfo,
            TDbContext dbContext
            )
            where TDbContext : MDbContext
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

            await SaveRefreshToken(loginReply.RefreshToken, dbContext, existedUser.EntityId, tokenValidate, mTokenInfo);

            await RemoveOldRefreshTokensByUser(existedUser.EntityId, dbContext, mTokenInfo);

            return loginReply;
        }

        private static bool IsAccountLocked(MUserLoginAttempt? loginAttemptHistory, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (loginAttemptHistory != null && loginAttemptHistory.LockTo > Clock.UtcNow)
            {
                TimeSpan remainingLockTime = loginAttemptHistory.LockTo - Clock.UtcNow;
                errorMessage = remainingLockTime.ToString();
                return true;
            }

            return false;
        }

        private static async Task ResetLoginAttemptOnSuccess<TDbContext>(MUser existedUser, MUserLoginAttempt? loginAttemptHistory,
           TDbContext dbContext, CancellationToken cancellationToken)
            where TDbContext : MDbContext
        {
            if (loginAttemptHistory != null)
            {
                loginAttemptHistory.AttemptTime = 0;
                loginAttemptHistory.LockTo = DateTime.MinValue;
                _ = dbContext.Update(loginAttemptHistory);
            }

            if (!existedUser.IsActive)
            {
                existedUser.IsActive = true;
                _ = dbContext.Users.Update(existedUser);
            }

            _ = await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task<List<TPermission>> GetPermissionsOfUser<TDbContext, TPermission>(long userId,
           TDbContext dbContext)
              where TDbContext : MDbContext
              where TPermission : Enum
        {
            List<string> permissionNames = await (from user in dbContext.Set<MUser>()
                                                  join userRole in dbContext.Set<MUserRole>() on user.EntityId equals userRole.UserId
                                                  join role in dbContext.Set<MRole>() on userRole.RoleId equals role.EntityId
                                                  join rolePermission in dbContext.Set<MRolePermission>() on role.EntityId equals rolePermission.RoleId
                                                  join permission in dbContext.Set<MPermission>() on rolePermission.PermissionId equals permission.EntityId
                                                  where user.Id == userId
                                                  select permission.Name).Distinct().ToListAsync();
            return permissionNames.Select(name => (TPermission)Enum.Parse(typeof(TPermission), name)).ToList();
        }


        private static void GenerateAccessToken<TPermission>(MUser user,
            List<TPermission> permissions,
            out string accessToken,
            out string tokenValidityKey,
            MTokenInfo mTokenInfo,
            MAuthenticateTokenHelper<TPermission> tokenHelper)
            where TPermission : Enum
        {
            tokenValidityKey = Guid.NewGuid().ToString();

            MUserModel userModel = new(user.EntityId.ToString(), user.UserName, tokenValidityKey);

            accessToken = tokenHelper.GenerateAuthenticateToken(userModel, permissions, Clock.UtcNow.AddMinutes(mTokenInfo.ExpiryMinutes));
        }

        private static void GenerateRefreshToken(out string refreshToken)
        {
            byte[] randomNumber = new byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            refreshToken = Convert.ToBase64String(randomNumber);
        }

        private static void GenerateAccessToken(List<Claim>? claims, out string accessToken, MTokenInfo mTokenInfo)
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
            accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static async Task SaveRefreshToken<TDbContext>(string refreshToken,
           TDbContext dbContext, Guid userId, string tokenValidityKey, MTokenInfo mTokenInfo)
              where TDbContext : MDbContext
        {
            MRefreshToken token = new()
            {
                Token = refreshToken,
                TokenValidityKey = tokenValidityKey,
                ExpiredDate = Clock.UtcNow.AddMinutes(mTokenInfo.RefreshTokenEIM),
                IsDeleted = false,
                IsRevoked = false,
                CreatorUserId = userId,
                CreationTime = Clock.UtcNow
            };
            _ = await dbContext.RefreshTokens.AddAsync(token);
            _ = await dbContext.SaveChangesAsync();
        }
        private static async Task RemoveOldRefreshTokensByUser<TDbContext>(Guid userId,
           TDbContext dbContext, MTokenInfo mTokenInfo)
                where TDbContext : MDbContext
        {
            List<MRefreshToken> tokensToDelete = [.. dbContext.RefreshTokens
            .Where(rt =>
            (rt.IsDeleted || rt.IsRevoked ) &&
             rt.CreationTime.AddDays(mTokenInfo.RefreshTokenTTL) <= Clock.UtcNow &&
                rt.CreatorUserId == userId)];

            if (tokensToDelete.Count != 0)
            {
                dbContext.RefreshTokens.RemoveRange(tokensToDelete);
                _ = await dbContext.SaveChangesAsync();
            }
        }
    }
}
