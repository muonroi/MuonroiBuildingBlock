namespace MuonroiBuildingBlock.Entity.EFConfig
{
    public class AppUserRoleConfiguration : IEntityTypeConfiguration<AppUserRole>
    {
        public void Configure(EntityTypeBuilder<AppUserRole> builder)
        {
            _ = builder.Ignore(x => x.Id);
            _ = builder.HasKey(x => new { x.UserId, x.RoleId });
        }
    }
}