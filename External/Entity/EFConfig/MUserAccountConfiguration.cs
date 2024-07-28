namespace MBuildingBlock.External.Entity.EFConfig
{
    public class MUserAccountConfiguration : IEntityTypeConfiguration<MUserAccount>
    {
        public void Configure(EntityTypeBuilder<MUserAccount> builder)
        {
            _ = builder.HasIndex(b => b.UserName)
                .HasDatabaseName("IX_UserName")
                .IsUnique();
        }
    }
}