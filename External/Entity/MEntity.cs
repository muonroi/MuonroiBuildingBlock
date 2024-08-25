namespace Muonroi.BuildingBlock.External.Entity
{
    public abstract class MEntity : MValidationObject
    {
        /// <summary>
        /// Maximum length of the <see cref="UserName"/> property.
        /// </summary>
        public const int MaxUserNameLength = 256;

        /// <summary>
        /// Maximum length of the <see cref="EmailAddress"/> property.
        /// </summary>
        public const int MaxEmailAddressLength = 256;

        /// <summary>
        /// Maximum length of the <see cref="Name"/> property.
        /// </summary>
        public const int MaxNameLength = 64;

        /// <summary>
        /// Maximum length of the <see cref="Surname"/> property.
        /// </summary>
        public const int MaxSurnameLength = 64;

        /// <summary>
        /// Maximum length of the <see cref="AuthenticationSource"/> property.
        /// </summary>
        public const int MaxAuthenticationSourceLength = 64;

        /// <summary>
        /// UserName of the admin.
        /// admin can not be deleted and UserName of the admin can not be changed.
        /// </summary>
        public const string AdminUserName = "admin";

        /// <summary>
        /// Maximum length of the <see cref="Password"/> property.
        /// </summary>
        public const int MaxPasswordLength = 128;

        /// <summary>
        /// Maximum length of the <see cref="Password"/> without hashed.
        /// </summary>
        public const int MaxPlainPasswordLength = 32;

        /// <summary>
        /// Maximum length of the <see cref="EmailConfirmationCode"/> property.
        /// </summary>
        public const int MaxEmailConfirmationCodeLength = 328;

        /// <summary>
        /// Maximum length of the <see cref="PasswordResetCode"/> property.
        /// </summary>
        public const int MaxPasswordResetCodeLength = 328;

        /// <summary>
        /// Maximum length of the <see cref="PhoneNumber"/> property.
        /// </summary>
        public const int MaxPhoneNumberLength = 32;

        /// <summary>
        /// Maximum length of the <see cref="SecurityStamp"/> property.
        /// </summary>
        public const int MaxSecurityStampLength = 128;

        private List<INotification> _domainEvents = [];

        private int? _requestedHashCode;
        public IReadOnlyCollection<INotification>? DomainEvents => _domainEvents.AsReadOnly();

        protected MEntity()
        {
            DateTime utcNow = DateTime.UtcNow;
            CreatedDateTS = utcNow.GetTimeStamp(includedTimeValue: true);
            EntityId = Guid.NewGuid();
        }

        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long Id { get; set; }

        [Column(Order = 1)]
        public virtual Guid EntityId { get; set; }

        [Column(Order = 101)]
        public int? CreatedUserId { get; set; }

        [Column(Order = 102)]
        public int? UpdatedUserId { get; set; }

        [Column(Order = 103)]
        public int? DeletedUserId { get; set; }

        [Column(Order = 104)]
        [MaxLength(100)]
        public string? CreatedUserName { get; set; }

        [Column(Order = 105)]
        [MaxLength(100)]
        public string? UpdatedUserName { get; set; }

        [Column(Order = 106)]
        [MaxLength(100)]
        public string? DeletedUserName { get; set; }

        [Column(Order = 107)]
        public double CreatedDateTS { get; set; }

        [Column(Order = 108)]
        public double? UpdatedDateTS { get; set; }

        [Column(Order = 109)]
        public double? DeletedDateTS { get; set; }

        [Column(Order = 110)]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        public void AddDomainEvent(INotification eventItem)
        {
            _domainEvents ??= [];
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            _ = (_domainEvents?.Remove(eventItem));
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public bool IsTransient()
        {
            return Id == 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null or not MEntity)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            MEntity entity = (MEntity)obj;
            return !entity.IsTransient() && !IsTransient() && entity.Id == Id;
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                {
                    _requestedHashCode = Id.GetHashCode() ^ 0x1F;
                }

                return _requestedHashCode.Value;
            }

            return base.GetHashCode();
        }
    }
}