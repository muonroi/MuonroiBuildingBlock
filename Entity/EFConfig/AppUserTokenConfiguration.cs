namespace MuonroiBuildingBlock.Entity.EFConfig
{
    public class AppUserTokenConfiguration : IEntityTypeConfiguration<AppUserToken>
    {
        public void Configure(EntityTypeBuilder<AppUserToken> builder)
        {
            _ = builder.HasIndex(b => b.LoginProvider).HasDatabaseName("IX_LoginProvider").IsUnique(false);
        }
    }
}