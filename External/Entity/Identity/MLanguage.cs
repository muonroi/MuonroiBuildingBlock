namespace MBuildingBlock.External.Entity.Identity
{
    [Table("MLanguages")]
    public class MLanguage : MEntity
    {
        /// <summary>
        /// The maximum name length.
        /// </summary>
        public new const int MaxNameLength = 128;

        /// <summary>
        /// The maximum display name length.
        /// </summary>
        public const int MaxDisplayNameLength = 64;

        /// <summary>
        /// The maximum icon length.
        /// </summary>
        public const int MaxIconLength = 128;

        /// <summary>
        /// Gets or sets the name of the culture, like "en" or "en-US".
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public virtual string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [Required]
        [StringLength(MaxDisplayNameLength)]
        public virtual string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        [StringLength(MaxIconLength)]
        public virtual string? Icon { get; set; } = string.Empty;

        /// <summary>
        /// Is this language active. Inactive languages are not get by <see cref="IMlicationLanguageManager"/>.
        /// </summary>
        public virtual bool IsDisabled { get; set; }

        /// <summary>
        /// Creates a new <see cref="MlicationLanguage"/> object.
        /// </summary>
        public MLanguage()
        {
        }

        public MLanguage(string name, string displayName, string? icon = null, bool isDisabled = false)
        {
            Name = name;
            DisplayName = displayName;
            Icon = icon;
            IsDisabled = isDisabled;
        }

        public virtual MLanguageInfo ToLanguageInfo()
        {
            return new MLanguageInfo(Name, DisplayName, Icon, isDisabled: IsDisabled);
        }
    }
}