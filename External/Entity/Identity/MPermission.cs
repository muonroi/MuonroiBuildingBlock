namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    [Table("MPermissions")]
    public class MPermission : MEntity
    {
        [Required]
        [StringLength(MaxNameLength)]
        public virtual string Name { get; set; } = string.Empty;

        public virtual bool IsGranted { get; set; } = true;

        [StringLength(255)]
        public virtual string Discriminator { get; set; } = string.Empty;
    }

}