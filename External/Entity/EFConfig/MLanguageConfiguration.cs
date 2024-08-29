namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class MLanguageConfiguration : IEntityTypeConfiguration<MLanguage>
    {
        public void Configure(EntityTypeBuilder<MLanguage> builder)
        {
            _ = builder.HasIndex(b => b.Name)
                .HasDatabaseName("IX_MLanguages_Name")
                .IsUnique();
        }
    }
}