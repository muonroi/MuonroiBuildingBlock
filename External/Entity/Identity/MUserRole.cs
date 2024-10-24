

namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    [Table("MUserRoles")]
    public class MUserRole : MEntity
    {
        [Required]
        public virtual Guid UserId { get; set; }

        [Required]
        public virtual Guid RoleId { get; set; }
    }

}