namespace Muonroi.BuildingBlock.External.Controller
{
    public class MAuthControllerBase<TPermission>(MDbContext dbContext, MAuthenticateTokenHelper<TPermission> tokenHelper) : ControllerBase
        where TPermission : Enum
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public virtual async Task<IActionResult> Login([FromBody] LoginRequestModel command)
        {
            MResponse<object> result = new();
            MUser? user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == command.Username);
            if (user == null || string.IsNullOrEmpty(user.Salt) || !MPasswordHelper.VerifyPassword(command.Password, user.Password, user.Salt))
            {
                result.AddApiErrorMessage(nameof(SystemEnum.InvalidCredentials), [command.Username]);
                return result.GetActionResult();
            }

            List<TPermission> permissions = await GetPermissionsOfUser(user.Id);
            GenerateToken(user, permissions, out string accessToken, out string refreshToken, out MUserToken loginToken);

            result.Result = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            _ = await dbContext.AddAsync(loginToken);
            _ = await dbContext.SaveChangesAsync();

            return result.GetActionResult();
        }

        [HttpPost("create-role")]
        public virtual async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestModel request)
        {
            MResponse<MRole> result = new();

            MRole? existingRole = await dbContext.Set<MRole>().SingleOrDefaultAsync(r => r.Name == request.Name);
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

            MRole? role = await dbContext.Set<MRole>().FindAsync(request.RoleId);
            MPermission? permission = await dbContext.Set<MPermission>().FindAsync(request.PermissionId);

            if (role == null || permission == null)
            {
                result.AddApiErrorMessage(role == null ? nameof(SystemEnum.RoleNotFound) : nameof(SystemEnum.PermissionNotFound), []);
                return result.GetActionResult();
            }

            MRolePermission rolePermission = new()
            {
                RoleId = role.EntityId,
                PermissionId = permission.EntityId
            };

            _ = await dbContext.Set<MRolePermission>().AddAsync(rolePermission);
            _ = await dbContext.SaveChangesAsync();

            return result.GetActionResult();
        }

        [HttpGet("user-permissions/{userId}")]
        public virtual async Task<IActionResult> GetUserPermissions(long userId)
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

            MRole? role = await dbContext.Set<MRole>().FindAsync(request.Id);
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
        public virtual async Task<IActionResult> DeleteRole(long roleId)
        {
            MResponse<object> result = new();

            MRole? role = await dbContext.Set<MRole>().FindAsync(roleId);
            if (role == null)
            {
                result.AddApiErrorMessage(nameof(SystemEnum.RoleNotFound), []);
                return result.GetActionResult();
            }

            _ = dbContext.Set<MRole>().Remove(role);
            _ = await dbContext.SaveChangesAsync();

            return result.GetActionResult();
        }

        [HttpGet("roles")]
        public virtual async Task<IActionResult> GetRoles()
        {
            MResponse<List<MRole>> result = new();

            List<MRole> roles = await dbContext.Set<MRole>().ToListAsync();
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
                                       select user).ToListAsync();

            result.Result = users;
            return result.GetActionResult();
        }

        private void GenerateToken(MUser user, List<TPermission> permissions, out string accessToken, out string refreshToken, out MUserToken loginToken)
        {
            DateTime refreshExpirationTime = DateTime.UtcNow.AddMinutes(525960);

            MUserModel userModel = new(user.EntityId.ToString(), user.UserName, [user.Name]);

            accessToken = tokenHelper.GenerateAuthenticateToken(userModel, permissions, DateTime.UtcNow.AddMinutes(15));

            refreshToken = Guid.NewGuid().ToString();

            loginToken = new(user.Id, "System", "RefreshToken", accessToken, refreshExpirationTime);
        }

        private async Task<List<TPermission>> GetPermissionsOfUser(long userId)
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

    }
}
