using Muonroi.BuildingBlock.External.Common.Enums;
using Muonroi.BuildingBlock.External.Entity;
using Muonroi.BuildingBlock.External.Timing;

namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    [Table("MUserLoginAttempts")]
    public class MUserLoginAttempt : MEntity
    {
        public const int MaxUserNameOrEmailAddressLength = MaxEmailAddressLength;

        /// <summary>
        /// Maximum length of <see cref="ClientIpAddress"/> property.
        /// </summary>
        public const int MaxClientIpAddressLength = 64;

        /// <summary>
        /// Maximum length of <see cref="ClientName"/> property.
        /// </summary>
        public const int MaxClientNameLength = 128;

        /// <summary>
        /// Maximum length of <see cref="BrowserInfo"/> property.
        /// </summary>
        public const int MaxBrowserInfoLength = 512;

        /// <summary>
        /// User's Id, if <see cref="UserNameOrEmailAddress"/> was a valid username or email address.
        /// </summary>
        public virtual long? UserId { get; set; }

        /// <summary>
        /// User name or email address
        /// </summary>
        [StringLength(MaxUserNameOrEmailAddressLength)]
        public virtual string UserNameOrEmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// IP address of the client.
        /// </summary>
        [StringLength(MaxClientIpAddressLength)]
        public virtual string ClientIpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Name (generally computer name) of the client.
        /// </summary>
        [StringLength(MaxClientNameLength)]
        public virtual string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// Browser information if this method is called in a web request.
        /// </summary>
        [StringLength(MaxBrowserInfoLength)]
        public virtual string BrowserInfo { get; set; } = string.Empty;

        /// <summary>
        /// Login attempt result.
        /// </summary>
        public virtual MLoginResultType Result { get; set; }

        public virtual DateTime CreationTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MUserLoginAttempt"/> class.
        /// </summary>
        public MUserLoginAttempt()
        {
            CreationTime = Clock.Now;
        }
    }
}