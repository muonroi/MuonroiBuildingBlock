namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    [Table("MRolePermissions")]
    public class MRolePermission : MEntity
    {
        [Required]
        public virtual Guid RoleId { get; set; }

        [Required]
        public virtual Guid PermissionId { get; set; }
    }
}
