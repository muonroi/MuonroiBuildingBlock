namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class MUserLoginAttemptConfiguration : IEntityTypeConfiguration<MUserLoginAttempt>
    {
        public void Configure(EntityTypeBuilder<MUserLoginAttempt> builder)
        {
            _ = builder.HasIndex(b => b.UserNameOrEmailAddress).HasDatabaseName("IX_UserNameOrEmailAddress").IsUnique(false);
        }
    }
}