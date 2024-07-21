namespace MuonroiBuildingBlock.Entity.Identity
{
    [Table("AppUserAccounts")]
    public class AppUserAccount : EntityBase
    {
        public virtual long UserId { get; set; }

        public virtual long? UserLinkId { get; set; }

        [StringLength(MaxUserNameLength)]
        public virtual string UserName { get; set; } = string.Empty;

        [StringLength(MaxEmailAddressLength)]
        public virtual string EmailAddress { get; set; } = string.Empty;
    }
}