namespace MuonroiBuildingBlock.Entity.Identity
{
    [Table("AppUserTokens")]
    public class AppUserToken : EntityBase
    {
        /// <summary>
        /// Maximum length of the <see cref="LoginProvider"/> property.
        /// </summary>
        public const int MaxLoginProviderLength = 128;

        /// <summary>
        /// Maximum length of the <see cref="Value"/> property.
        /// </summary>
        public const int MaxValueLength = 512;

        public long UserId { get; set; }

        [StringLength(MaxLoginProviderLength)]
        public virtual string LoginProvider { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the token.
        /// </summary>
        [StringLength(MaxNameLength)]
        public virtual string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token value.
        /// </summary>
        [StringLength(MaxValueLength)]
        public virtual string Value { get; set; } = string.Empty;

        public DateTime? ExpireDate { get; set; }

        protected AppUserToken()
        {
        }

        protected internal AppUserToken(AppUserToken user, [NotNull] string loginProvider, [NotNull] string name, string value, DateTime? expireDate = null)
        {
            _ = Check.NotNull(loginProvider, nameof(loginProvider));
            _ = Check.NotNull(name, nameof(name));
            UserId = user.Id;
            LoginProvider = loginProvider;
            Name = name;
            Value = value;
            ExpireDate = expireDate;
        }
    }
}