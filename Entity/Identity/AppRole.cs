namespace MuonroiBuildingBlock.Entity.Identity
{
    [Table("AppRoles")]
    public class AppRole : EntityBase
    {
        public AppRole()
        {
            Name = Guid.NewGuid().ToString("N");
            SetNormalizedName();
        }

        public AppRole(string displayName) : this()
        {
            DisplayName = displayName;
        }

        public AppRole(string name, string displayName) : this(displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        /// <summary>
        /// Maximum length of the <see cref="ConcurrencyStamp"/> property.
        /// </summary>
        public const int MaxConcurrencyStampLength = 128;

        public virtual string Name { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;
        public virtual bool IsStatic { get; set; }
        public virtual bool IsDefault { get; set; }

        [Required]
        [StringLength(MaxNameLength)]
        public virtual string NormalizedName { get; set; } = string.Empty;

        [StringLength(MaxConcurrencyStampLength)]
        public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        public virtual void SetNormalizedName()
        {
            NormalizedName = Name.ToUpperInvariant();
        }

        public override string ToString()
        {
            return $"[Role {Id}, Name={Name}]";
        }
    }
}