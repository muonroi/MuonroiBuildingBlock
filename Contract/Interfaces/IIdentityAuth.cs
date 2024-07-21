namespace MuonroiBuildingBlock.Contract.Interfaces
{
    public interface IIdentityAuth
    {
        DbSet<AppUserAccount> UserAccounts { get; set; }
        DbSet<AppUser> Users { get; set; }
        DbSet<AppRole> Roles { get; set; }
        DbSet<AppPermission> Permissions { get; set; }
        DbSet<AppUserRole> UserRoles { get; set; }
        DbSet<AppLanguage> Languages { get; set; }
        DbSet<AppUserLogin> UserLogins { get; set; }
        DbSet<AppUserToken> UserTokens { get; set; }
        DbSet<AppUserLoginAttempt> AppUserLoginAttempts { get; set; }
    }
}