namespace MuonroiBuildingBlock.Entity.EFConfig
{
    public class AppUserLoginAttemptConfiguration : IEntityTypeConfiguration<AppUserLoginAttempt>
    {
        public void Configure(EntityTypeBuilder<AppUserLoginAttempt> builder)
        {
            _ = builder.HasIndex(b => b.UserNameOrEmailAddress).HasDatabaseName("IX_UserNameOrEmailAddress").IsUnique(false);
        }
    }
}