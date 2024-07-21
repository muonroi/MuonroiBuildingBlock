namespace MuonroiBuildingBlock.Entity.EFConfig
{
    public class AppPermissionConfiguration : IEntityTypeConfiguration<AppPermission>
    {
        public void Configure(EntityTypeBuilder<AppPermission> builder)
        {
            _ = builder.HasIndex(b => b.Name)
                .HasDatabaseName("IX_Name")
                .IsUnique();
        }
    }
}