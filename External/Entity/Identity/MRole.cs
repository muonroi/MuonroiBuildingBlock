

namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    [Table("MRoles")]
    public class MRole : MEntity
    {
        [Required]
        [StringLength(MaxNameLength)]
        public virtual string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(MaxNameLength)]
        public virtual string DisplayName { get; set; } = string.Empty;

        [Required]
        [StringLength(MaxNameLength)]
        public virtual string NormalizedName { get; set; } = string.Empty;

        public virtual bool IsStatic { get; set; } = false;
        public virtual bool IsDefault { get; set; } = false;
        [StringLength(128)]
        public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }

}