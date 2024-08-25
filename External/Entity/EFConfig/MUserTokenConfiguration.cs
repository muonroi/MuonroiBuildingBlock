namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class MUserTokenConfiguration : IEntityTypeConfiguration<MUserToken>
    {
        public void Configure(EntityTypeBuilder<MUserToken> builder)
        {
            _ = builder.HasIndex(b => b.LoginProvider).HasDatabaseName("IX_LoginProvider").IsUnique(false);
        }
    }
}