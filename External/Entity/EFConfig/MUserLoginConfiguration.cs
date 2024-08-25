namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class MUserLoginConfiguration : IEntityTypeConfiguration<MUserLogin>
    {
        public void Configure(EntityTypeBuilder<MUserLogin> builder)
        {
            _ = builder.HasIndex(b => b.UserId).HasDatabaseName("IX_UserId").IsUnique();
        }
    }
}