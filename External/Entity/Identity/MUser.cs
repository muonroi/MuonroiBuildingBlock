

namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    [Table("MUsers")]
    public class MUser : MEntity
    {
        public const int MaxConcurrencyStampLength = 128;

        public MUser()
        {
            IsTwoFactorEnabled = false;
            IsActive = true;
            SecurityStamp = MSequentialGuidGenerator.Instance.Create().ToString();
        }

        private string _userName = string.Empty;

        [Required]
        [StringLength(MaxUserNameLength)]
        public virtual string UserName
        {
            get => _userName;
            set
            {
                _userName = value ?? string.Empty;
                NormalizedUserName = _userName.ToUpperInvariant();
            }
        }

        private string _emailAddress = string.Empty;

        [Required]
        [StringLength(MaxEmailAddressLength)]
        public virtual string EmailAddress
        {
            get => _emailAddress;
            set
            {
                _emailAddress = value ?? string.Empty;
                NormalizedEmailAddress = _emailAddress.ToUpperInvariant();
            }
        }

        [Required]
        [StringLength(MaxNameLength)]
        public virtual string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(MaxSurnameLength)]
        public virtual string Surname { get; set; } = string.Empty;

        [NotMapped]
        public virtual string FullName => Name + " " + Surname;

        [Required]
        [StringLength(MaxPasswordLength)]
        public virtual string Password { get; set; } = string.Empty;

        public virtual string? PasswordResetCode { get; set; }

        [StringLength(MaxPhoneNumberLength)]
        public virtual string PhoneNumber { get; set; } = string.Empty;

        public virtual bool IsPhoneNumberConfirmed { get; set; }

        [StringLength(MaxSecurityStampLength)]
        public virtual string? SecurityStamp { get; set; }

        public virtual bool IsTwoFactorEnabled { get; set; }

        public virtual bool IsEmailConfirmed { get; set; }

        public virtual bool IsActive { get; set; }

        [Required]
        [StringLength(MaxUserNameLength)]
        public virtual string NormalizedUserName { get; private set; } = string.Empty;

        [Required]
        [StringLength(MaxEmailAddressLength)]
        public virtual string NormalizedEmailAddress { get; private set; } = string.Empty;

        [StringLength(MaxConcurrencyStampLength)]
        public virtual string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        public virtual string? Salt { get; set; }

        public virtual int? ProfilePictureId { get; set; }

        public virtual bool ShouldChangePasswordOnNextLogin { get; set; }
        public virtual DateTime? SignInTokenExpireTimeUtc { get; set; }

        public virtual void SetNewPasswordResetCode()
        {
            PasswordResetCode = Guid.NewGuid().ToString("N").Truncate(10)?.ToUpperInvariant();
        }
    }
}