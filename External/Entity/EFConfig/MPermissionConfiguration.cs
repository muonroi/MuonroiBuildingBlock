namespace MBuildingBlock.External.Entity.EFConfig
{
    public class MPermissionConfiguration : IEntityTypeConfiguration<MPermission>
    {
        public void Configure(EntityTypeBuilder<MPermission> builder)
        {
            _ = builder.HasIndex(b => b.Name)
                .HasDatabaseName("IX_Name")
                .IsUnique();
        }
    }
}