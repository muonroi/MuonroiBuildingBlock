namespace Muonroi.BuildingBlock.External.Entity.DataSample
{
    public class HostRoleAndUserCreator<TContext>(TContext context)
        where TContext : MDbContext
    {
        public void Create()
        {
            CreateHostRoleAndUsers();
        }

        private void CreateHostRoleAndUsers()
        {
            //Admin role for host
            MRole? adminRoleForHost = context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.Name == StaticRoleAndUserNames.Host.Admin);
            if (adminRoleForHost == null)
            {
                adminRoleForHost = context.Roles.Add(new MRole(StaticRoleAndUserNames.Host.Admin, StaticRoleAndUserNames.Host.Admin)
                { IsStatic = true, IsDefault = true, CreatedDateTS = DateTime.UtcNow.GetTimeStamp() }).Entity;

                _ = context.Roles.Add(new MRole(StaticRoleAndUserNames.Host.User, StaticRoleAndUserNames.Host.User)
                { IsStatic = true, IsDefault = true, CreatedDateTS = DateTime.UtcNow.GetTimeStamp() });
                _ = context.SaveChanges();
            }

            MUser? adminUserForHost = context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.UserName == StaticRoleAndUserNames.Host.AdminUserName);
            if (adminUserForHost is null)
            {
                MUser user = new()
                {
                    UserName = StaticRoleAndUserNames.Host.AdminUserName,
                    Name = "admin",
                    Surname = "admin",
                    EmailAddress = "admin@muonroi.com",
                    IsEmailConfirmed = true,
                    ShouldChangePasswordOnNextLogin = false,
                    IsActive = true,
                    Password = "$2b$12$pZk2J7RAK/kAjozBMN.AkOIm4SD9OdO.bODsZ/G2LZjHbpGjD/nli", //123qwe,
                    Salf = "$2b$12$pZk2J7RAK/kAjozBMN.AkO",
                    CreationTime = DateTime.UtcNow
                };

                user.SetNormalizedNames();

                adminUserForHost = context.Users.Add(user).Entity;
                _ = context.SaveChanges();

                _ = context.UserRoles.Add(new MUserRole(adminUserForHost.Id, adminRoleForHost.Id));
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
    }
}