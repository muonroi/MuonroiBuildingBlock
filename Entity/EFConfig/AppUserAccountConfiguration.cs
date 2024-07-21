namespace MuonroiBuildingBlock.Entity.EFConfig
{
    public class AppUserAccountConfiguration : IEntityTypeConfiguration<AppUserAccount>
    {
        public void Configure(EntityTypeBuilder<AppUserAccount> builder)
        {
            _ = builder.HasIndex(b => b.UserName)
                .HasDatabaseName("IX_UserName")
                .IsUnique();
        }
    }
}