namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class MUserConfiguration : IEntityTypeConfiguration<MUser>
    {
        public void Configure(EntityTypeBuilder<MUser> builder)
        {
            _ = builder.HasIndex(b => b.UserName)
                .HasDatabaseName("IX_UserName").IsUnique();
        }
    }
}