namespace MuonroiBuildingBlock.Entity.EFConfig
{
    public class AppLanguageConfiguration : IEntityTypeConfiguration<AppLanguage>
    {
        public void Configure(EntityTypeBuilder<AppLanguage> builder)
        {
            _ = builder.HasIndex(b => b.Name)
                .HasDatabaseName("IX_Name")
                .IsUnique();
        }
    }
}