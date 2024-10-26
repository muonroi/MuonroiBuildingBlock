

namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    [Table("MUsers")]
    public class MUser : MEntity
    {
        public MUser()
        {
            IsLockoutEnabled = true;
            IsTwoFactorEnabled = true;
            IsActive = true;
            SecurityStamp = MSequentialGuidGenerator.Instance.Create().ToString();
        }

        public const int MaxConcurrencyStampLength = 128;

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

        [StringLength(MaxEmailConfirmationCodeLength)]
        public virtual string? EmailConfirmationCode { get; set; }

        [StringLength(MaxPasswordResetCodeLength)]
        public virtual string? PasswordResetCode { get; set; }

        public virtual DateTime? LockoutEndDateUtc { get; set; }
        public virtual int AccessFailedCount { get; set; }
        public virtual bool IsLockoutEnabled { get; set; }

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
        public virtual string? SignInToken { get; set; }
        public virtual string? GoogleAuthenticatorKey { get; set; }
        public virtual string? RecoveryCode { get; set; }

        [StringLength(MaxAuthenticationSourceLength)]
        public virtual string? AuthenticationSource { get; set; }

        public virtual void SetNewPasswordResetCode()
        {
            PasswordResetCode = Guid.NewGuid().ToString("N").Truncate(10)?.ToUpperInvariant();
        }

        public virtual void Unlock()
        {
            AccessFailedCount = 0;
            LockoutEndDateUtc = null;
        }

        public virtual void SetSignInToken()
        {
            SignInToken = Guid.NewGuid().ToString();
            SignInTokenExpireTimeUtc = Clock.Now.AddMinutes(1).ToUniversalTime();
        }

        public virtual void SetNewEmailConfirmationCode()
        {
            EmailConfirmationCode = Guid.NewGuid().ToString("N").Truncate(MaxEmailConfirmationCodeLength);
        }
    }
}