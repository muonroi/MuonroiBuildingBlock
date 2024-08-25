namespace Muonroi.BuildingBlock.External.Entity.EFConfig
{
    public class MUserRoleConfiguration : IEntityTypeConfiguration<MUserRole>
    {
        public void Configure(EntityTypeBuilder<MUserRole> builder)
        {
            _ = builder.Ignore(x => x.Id);
            _ = builder.HasKey(x => new { x.UserId, x.RoleId });
        }
    }
}