namespace MuonroiBuildingBlock.Entity.Identity
{
    [Table("AppUserRoles")]
    public class AppUserRole : EntityBase
    {
        /// <summary>
        /// User id.
        /// </summary>
        public virtual long UserId { get; set; }

        /// <summary>
        /// Role id.
        /// </summary>
        public virtual int RoleId { get; set; }

        /// <summary>
        /// Creates a new <see cref="AppUserRole"/> object.
        /// </summary>
        public AppUserRole()
        {
        }

        /// <summary>
        /// Creates a new <see cref="AppUserRole"/> object.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="roleId">Role id</param>
        public AppUserRole(long userId, long roleId)
        {
            UserId = userId;
            RoleId = (int)roleId;
        }
    }
}