namespace MuonroiBuildingBlock.Entity.EFConfig
{
    public class AppUserLoginConfiguration : IEntityTypeConfiguration<AppUserLogin>
    {
        public void Configure(EntityTypeBuilder<AppUserLogin> builder)
        {
            _ = builder.HasIndex(b => b.UserId).HasDatabaseName("IX_UserId").IsUnique();
        }
    }
}