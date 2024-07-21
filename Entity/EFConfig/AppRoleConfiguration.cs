namespace MuonroiBuildingBlock.Entity.EFConfig
{
    public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
    {
        public void Configure(EntityTypeBuilder<AppRole> builder)
        {
            _ = builder.HasIndex(b => b.Name)
                .HasDatabaseName("IX_Name")
                .IsUnique();
        }
    }
}