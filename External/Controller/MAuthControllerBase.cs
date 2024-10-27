namespace Muonroi.BuildingBlock.External.Controller
{
    public class MAuthControllerBase<TPermission>(MDbContext dbContext,
        MAuthenticateInfoContext context,
        IAuthenticateRepository authenticateRepository) : ControllerBase
        where TPermission : Enum
    {
        [HttpPost("create-role")]
        public virtual async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestModel request)
        {
            MResponse<MRole> result = new();

            MRole? existingRole = await dbContext.Set<MRole>().SingleOrDefaultAsync(r => r.Name == request.Name && !r.IsDeleted);
            if (existingRole != null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.RoleAlreadyExists), [existingRole.Name]);
                return result.GetActionResult();
            }

            MRole role = new()
            {
                Name = request.Name,
                DisplayName = request.DisplayName,
                IsStatic = request.IsStatic,
                IsDefault = request.IsDefault,
                NormalizedName = request.Name.ToUpperInvariant()
            };

            _ = await dbContext.Set<MRole>().AddAsync(role);
            _ = await dbContext.SaveChangesAsync();

            result.Result = role;
            return result.GetActionResult();
        }

        [HttpPost("assign-permission")]
        public virtual async Task<IActionResult> AssignPermissionToRole([FromBody] AssignPermissionRequestModel request)
        {
            MResponse<object> result = new();

            MRole? role = await dbContext.Set<MRole>().FirstOrDefaultAsync(x => x.EntityId == request.RoleId && !x.IsDeleted);
            MPermission? permission = await dbContext.Set<MPermission>().FirstOrDefaultAsync(x => x.EntityId == request.PermissionId);

            if (role == null || permission == null)
            {
                result.AddApiErrorMessage(role == null ? nameof(SystemEnum.RoleNotFound) : nameof(SystemEnum.PermissionNotFound), []);
                return result.GetActionResult();
            }
            MRolePermission? rolePermission = await dbContext.Set<MRolePermission>().FirstOrDefaultAsync(x => x.RoleId == role.EntityId && x.PermissionId == permission.EntityId);
            if (rolePermission != null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.RoleAlreadyHasPermission), []);
                return result.GetActionResult();
            }

            MRolePermission newRolePermission = new()
            {
                RoleId = role.EntityId,
                PermissionId = permission.EntityId
            };

            _ = await dbContext.Set<MRolePermission>().AddAsync(newRolePermission);
            _ = await dbContext.SaveChangesAsync();

            return result.GetActionResult();
        }

        [HttpDelete("remove-permission/{roleId}/{permissionId}")]
        public virtual async Task<IActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
        {
            MResponse<object> result = new();
            MRolePermission? rolePermission = await dbContext.Set<MRolePermission>().FirstOrDefaultAsync(x => x.RoleId == roleId
            && x.PermissionId == permissionId && !x.IsDeleted);
            if (rolePermission == null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.RolePermissionNotFound), []);
                return result.GetActionResult();
            }

            rolePermission.IsDeleted = true;

            _ = await dbContext.SaveChangesAsync();
            return result.GetActionResult();
        }

        [HttpPost("assign-role")]
        public virtual async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleRequestModel request)
        {
            MResponse<object> result = new();
            MRole? role = await dbContext.Set<MRole>().FirstOrDefaultAsync(x => x.EntityId == request.RoleId && !x.IsDeleted);
            MUser? user = await dbContext.Set<MUser>().FirstOrDefaultAsync(x => x.EntityId == request.UserId && !x.IsDeleted);
            if (role == null || user == null)
            {
                result.AddApiErrorMessage(role == null ? nameof(SystemEnum.RoleNotFound) : nameof(SystemEnum.UserNotFound), []);
                return result.GetActionResult();
            }
            MUserRole? userRole = await dbContext.Set<MUserRole>().FirstOrDefaultAsync(x => x.RoleId == role.EntityId && x.UserId
            == user.EntityId);
            if (userRole != null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.UserAlreadyHasRole), []);
                return result.GetActionResult();
            }

            MUserRole newUserRole = new()
            {
                RoleId = role.EntityId,
                UserId = user.EntityId
            };
            _ = await dbContext.Set<MUserRole>().AddAsync(newUserRole);

            _ = await dbContext.SaveChangesAsync();

            return result.GetActionResult();
        }


        [HttpGet("user-permissions/{userId}")]
        public virtual async Task<IActionResult> GetUserPermissions(Guid userId)
        {
            MResponse<List<TPermission>> result = new();

            List<TPermission> userPermissions = await GetPermissionsOfUser(userId);
            if (userPermissions.Count == 0)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.UserHasNoPermissions), []);
            }
            else
            {
                result.Result = userPermissions;
            }

            return result.GetActionResult();
        }

        [HttpPost("update-role")]
        public virtual async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequestModel request)
        {
            MResponse<MRole> result = new();

            MRole? role = await dbContext.Set<MRole>().FirstOrDefaultAsync(x => x.EntityId == request.Id && !x.IsDeleted);
            if (role == null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.RoleNotFound), []);
                return result.GetActionResult();
            }

            role.Name = request.Name;
            role.DisplayName = request.DisplayName;
            role.IsStatic = request.IsStatic;
            role.IsDefault = request.IsDefault;
            role.NormalizedName = request.Name.ToUpperInvariant();

            _ = dbContext.Set<MRole>().Update(role);
            _ = await dbContext.SaveChangesAsync();

            result.Result = role;
            return result.GetActionResult();
        }

        [HttpDelete("delete-role/{roleId}")]
        public virtual async Task<IActionResult> DeleteRole(Guid roleId)
        {
            MResponse<object> result = new();

            MRole? role = await dbContext.Set<MRole>().FirstOrDefaultAsync(x => x.EntityId == roleId);
            if (role == null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.RoleNotFound), []);
                return result.GetActionResult();
            }
            role.IsDeleted = true;

            _ = await dbContext.SaveChangesAsync();

            return result.GetActionResult();
        }

        [HttpGet("roles")]
        public virtual async Task<IActionResult> GetRoles()
        {
            MResponse<List<MRole>> result = new();

            List<MRole> roles = await dbContext.Set<MRole>().Where(x => !x.IsDeleted).ToListAsync();
            result.Result = roles;

            return result.GetActionResult();
        }

        [HttpGet("permissions")]
        public virtual async Task<IActionResult> GetPermission()
        {
            MResponse<List<MPermission>> result = new();

            List<MPermission> roles = await dbContext.Set<MPermission>().Where(x => !x.IsDeleted).ToListAsync();
            result.Result = roles;

            return result.GetActionResult();
        }

        [HttpGet("role-permissions/{roleId}")]
        public virtual async Task<IActionResult> GetRolePermissions(Guid roleId)
        {
            MResponse<List<MPermission>> result = new();

            List<MPermission> permissions = await (from role in dbContext.Set<MRole>()
                                                   join rolePermission in dbContext.Set<MRolePermission>() on role.EntityId equals rolePermission.RoleId
                                                   join permission in dbContext.Set<MPermission>() on rolePermission.PermissionId equals permission.EntityId
                                                   where role.EntityId == roleId
                                                   && !permission.IsDeleted
                                                   && !rolePermission.IsDeleted
                                                   && !role.IsDeleted
                                                   select permission).ToListAsync();

            result.Result = permissions;
            return result.GetActionResult();
        }

        [HttpGet("role-users/{roleId}")]
        public virtual async Task<IActionResult> GetRoleUsers(Guid roleId)
        {
            MResponse<List<MUser>> result = new();

            List<MUser> users = await (from role in dbContext.Set<MRole>()
                                       join userRole in dbContext.Set<MUserRole>() on role.EntityId equals userRole.RoleId
                                       join user in dbContext.Set<MUser>() on userRole.UserId equals user.EntityId
                                       where role.EntityId == roleId
                                        && !user.IsDeleted
                                        && !role.IsDeleted
                                        && !userRole.IsDeleted
                                       select user).ToListAsync();

            result.Result = users;
            return result.GetActionResult();
        }

        [HttpPost("logout")]
        public virtual async Task<IActionResult> Logout()
        {
            MResponse<object> result = new();
            MUser? user = await dbContext.Set<MUser>().FirstOrDefaultAsync(x => x.EntityId == Guid.Parse(context.CurrentUserGuid));
            if (user == null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.UserNotFound), []);
                return result.GetActionResult();
            }
            MRefreshToken? mRefreshToken = await dbContext.Set<MRefreshToken>().FirstOrDefaultAsync(rt => rt.TokenValidityKey ==
            context.TokenValidityKey
            && !rt.IsDeleted
            && !rt.IsRevoked);
            if (mRefreshToken == null)
            {
                result.AddErrorMessage(nameof(SystemEnum.InvalidCredentials));
                return result.GetActionResult();
            }
            mRefreshToken.IsRevoked = true;
            mRefreshToken.RevokedDate = Clock.UtcNow;
            mRefreshToken.ReasonRevoked = "Logout";
            _ = dbContext.Set<MRefreshToken>().Update(mRefreshToken);
            _ = await dbContext.SaveChangesAsync();
            return result.GetActionResult();
        }

        [HttpPost("register")]
        public virtual async Task<IActionResult> Register([FromBody] RegisterRequestModel request, CancellationToken cancellationToken)
        {
            MResponse<LoginResponseModel> result = new();
            MUser? existingUser = await dbContext.Set<MUser>().SingleOrDefaultAsync(u => u.UserName == request.UserName,
                cancellationToken: cancellationToken);
            if (existingUser != null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.UserAlreadyExists), [existingUser.UserName]);
                return result.GetActionResult();
            }
            MUser user = new()
            {
                UserName = request.UserName,
                EmailAddress = request.Email,
                Password = MPasswordHelper.HashPassword(request.Password, out string salt),
                Salt = salt,
                Name = request.Name,
                Surname = request.Surname,
                PhoneNumber = request.PhoneNumber,
                IsActive = request.IsActive,
                IsTwoFactorEnabled = request.IsTwoFactorEnabled
            };
            _ = await dbContext.Set<MUser>().AddAsync(user, cancellationToken);

            _ = await dbContext.SaveChangesAsync(cancellationToken);

            MResponse<LoginResponseModel> loginResult = await authenticateRepository.Login(new LoginRequestModel
            {
                Username = request.UserName,
                Password = request.Password
            }, cancellationToken);
            if (loginResult.Result is null || !loginResult.IsOK)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.InvalidCredentials), [user.UserName]);
                return result.GetActionResult();
            }

            result.Result = new LoginResponseModel
            {
                AccessToken = loginResult.Result.AccessToken,
                RefreshToken = loginResult.Result.RefreshToken,
                EmailAddress = user.EmailAddress,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                IsEmailConfirmed = user.IsEmailConfirmed,
                IsPhoneNumberConfirmed = user.IsPhoneNumberConfirmed
            };
            return result.GetActionResult();
        }

        private async Task<List<TPermission>> GetPermissionsOfUser(Guid userId)
        {
            List<string> permissionNames = await (from user in dbContext.Set<MUser>()
                                                  join userRole in dbContext.Set<MUserRole>() on user.EntityId equals userRole.UserId
                                                  join role in dbContext.Set<MRole>() on userRole.RoleId equals role.EntityId
                                                  join rolePermission in dbContext.Set<MRolePermission>() on role.EntityId
                                                  equals rolePermission.RoleId
                                                  join permission in dbContext.Set<MPermission>()
                                                  on rolePermission.PermissionId equals permission.EntityId
                                                  where user.EntityId == userId
                                                    && !permission.IsDeleted
                                                    && !rolePermission.IsDeleted
                                                    && !role.IsDeleted
                                                    && !user.IsDeleted
                                                  select permission.Name).Distinct().ToListAsync();

            return permissionNames.Select(name => (TPermission)Enum.Parse(typeof(TPermission), name)).ToList();
        }

    }
}
