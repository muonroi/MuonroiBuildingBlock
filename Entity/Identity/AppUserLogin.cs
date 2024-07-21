namespace MuonroiBuildingBlock.Entity.Identity
{
    [Table("AppUserLogins")]
    public class AppUserLogin : EntityBase
    {
        /// <summary>
        /// Maximum length of <see cref="LoginProvider"/> property.
        /// </summary>
        public const int MaxLoginProviderLength = 128;

        /// <summary>
        /// Maximum length of <see cref="ProviderKey"/> property.
        /// </summary>
        public const int MaxProviderKeyLength = 256;

        public virtual long UserId { get; set; }

        /// <summary>
        /// Login Provider.
        /// </summary>
        [Required]
        [StringLength(MaxLoginProviderLength)]
        public virtual string LoginProvider { get; set; } = string.Empty;

        /// <summary>
        /// Key in the <see cref="LoginProvider"/>.
        /// </summary>
        [Required]
        [StringLength(MaxProviderKeyLength)]
        public virtual string ProviderKey { get; set; } = string.Empty;

        public AppUserLogin()
        {
        }

        public AppUserLogin(long userId, string loginProvider, string providerKey)
        {
            UserId = userId;
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
        }
    }
}