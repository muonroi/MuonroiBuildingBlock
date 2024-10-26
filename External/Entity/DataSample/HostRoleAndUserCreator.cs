namespace Muonroi.BuildingBlock.External.Entity.DataSample
{
    public class HostRoleAndUserCreator<TContext>(TContext context) where TContext : MDbContext
    {
        private readonly TContext context = context;

        public void Create()
        {
            List<string> permissionName =
            [
                "Auth_CreateRole",
                "Auth_UpdateRole",
                "Auth_DeleteRole",
                "Auth_GetRoles",
                "Auth_GetRoleById",
                "Auth_AssignPermission",
                "Auth_GetPermissions",
                "Auth_GetRolePermissions",
                "Auth_GetRoleUsers"
            ];
            if (!context.Users.Any(u => u.UserName == StaticRoleAndUserNames.Host.AdminUserName))
            {
                CreateHostRoleAndUsers();
            }

            if (!context.Set<MRole>().Any(r => r.Name == "Admin"))
            {
                CreateDefaultRolesAndPermissions();
            }

            if (!context.Set<MRolePermission>().Any())
            {
                AssignPermissionsToRoles(permissionName);
            }
        }

        private void CreateHostRoleAndUsers()
        {
            MUser? adminUserForHost = context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.UserName == StaticRoleAndUserNames.Host.AdminUserName);
            if (adminUserForHost is null)
            {
                MUser user = new()
                {
                    UserName = StaticRoleAndUserNames.Host.AdminUserName,
                    Name = "Admin",
                    Surname = "User",
                    EmailAddress = "admin@muonroi.com",
                    Password = "$2b$08$OT6ZsF1WkYzf8e6R9kwP8uVxLbKaff/jkGgarg42LUbtCcTAbBr3.", //123qWE,
                    IsEmailConfirmed = true,
                    ShouldChangePasswordOnNextLogin = false,
                    IsActive = true,
                    Salt = "$2b$08$OT6ZsF1WkYzf8e6R9kwP8u",
                    CreationTime = DateTime.UtcNow
                };

                adminUserForHost = context.Users.Add(user).Entity;
                _ = context.SaveChanges();

                _ = context.UserAccounts.Add(new MUserAccount
                {
                    UserId = adminUserForHost.Id,
                    UserName = StaticRoleAndUserNames.Host.AdminUserName,
                    EmailAddress = adminUserForHost.EmailAddress,
                    CreatedDateTS = DateTime.UtcNow.GetTimeStamp()
                });

                _ = context.SaveChanges();
            }
        }

        private void CreateDefaultRolesAndPermissions()
        {
            MRole adminRole = new()
            {
                Name = "Admin",
                DisplayName = "Administrator",
                NormalizedName = "ADMIN",
                IsStatic = true,
                IsDefault = true
            };

            if (context.Set<MRole>().Any(r => r.Name == adminRole.Name))
            {
                return;
            }

            _ = context.Set<MRole>().Add(adminRole);
            _ = context.SaveChanges();

            List<MPermission> permissions =
            [
                new() { Name = "Auth_CreateRole", IsGranted = true },
                new() { Name = "Auth_UpdateRole", IsGranted = true },
                new() { Name = "Auth_DeleteRole", IsGranted = true },
                new() { Name = "Auth_GetRoles", IsGranted = true },
                new() { Name = "Auth_GetRoleById", IsGranted = true },
                new() { Name = "Auth_AssignPermission", IsGranted = true },
                new() { Name = "Auth_GetPermissions", IsGranted = true },
                new() { Name = "Auth_GetRolePermissions", IsGranted = true },
                new() { Name = "Auth_GetRoleUsers", IsGranted = true }

            ];

            foreach (MPermission permission in permissions)
            {
                if (!context.Set<MPermission>().Any(p => p.Name == permission.Name))
                {
                    _ = context.Set<MPermission>().Add(permission);
                }
            }

            _ = context.SaveChanges();
        }


        private void AssignPermissionsToRoles(IEnumerable<string> permissionNames)
        {
            MRole? adminRole = context.Set<MRole>().FirstOrDefault(r => r.Name == "Admin");

            List<MPermission> permissions = [.. context.Set<MPermission>().Where(p => permissionNames.Contains(p.Name))];

            if (adminRole != null)
            {
                foreach (MPermission? permission in permissions)
                {
                    _ = context.Set<MRolePermission>().Add(new MRolePermission
                    {
                        RoleId = adminRole.EntityId,
                        PermissionId = permission.EntityId
                    });
                }

                MUser? adminUser = context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.UserName == StaticRoleAndUserNames.Host.AdminUserName);
                if (adminUser != null)
                {
                    bool existingUserRole = context.Set<MUserRole>().Any(ur => ur.UserId == adminUser.EntityId && ur.RoleId == adminRole.EntityId);
                    if (!existingUserRole)
                    {
                        _ = context.Set<MUserRole>().Add(new MUserRole
                        {
                            UserId = adminUser.EntityId,
                            RoleId = adminRole.EntityId
                        });
                    }
                }
            }

            _ = context.SaveChanges();
        }

    }
}
