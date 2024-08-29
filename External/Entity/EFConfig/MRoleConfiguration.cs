namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class MRoleConfiguration : IEntityTypeConfiguration<MRole>
    {
        public void Configure(EntityTypeBuilder<MRole> builder)
        {
            _ = builder.HasIndex(b => b.Name)
                .HasDatabaseName("IX_MRoles_Name")
                .IsUnique();
        }
    }
}