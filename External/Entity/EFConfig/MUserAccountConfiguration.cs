namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class MUserAccountConfiguration : IEntityTypeConfiguration<MUserAccount>
    {
        public void Configure(EntityTypeBuilder<MUserAccount> builder)
        {
            _ = builder.HasIndex(b => b.UserName)
                .HasDatabaseName("IX_MUserAccount_UserName")
                .IsUnique();
        }
    }
}