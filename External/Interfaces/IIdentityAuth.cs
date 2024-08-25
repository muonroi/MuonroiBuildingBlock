namespace Muonroi.BuildingBlock.External.Interfaces
{
    public interface IIdentityAuth
    {
        DbSet<MUserAccount> UserAccounts { get; set; }
        DbSet<MUser> Users { get; set; }
        DbSet<MRole> Roles { get; set; }
        DbSet<MPermission> Permissions { get; set; }
        DbSet<MUserRole> UserRoles { get; set; }
        DbSet<MLanguage> Languages { get; set; }
        DbSet<MUserLogin> UserLogins { get; set; }
        DbSet<MUserToken> UserTokens { get; set; }
        DbSet<MUserLoginAttempt> MUserLoginAttempts { get; set; }
    }
}