using Muonroi.BuildingBlock.External.Entity;

namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    [Table("MUserRoles")]
    public class MUserRole : MEntity
    {
        /// <summary>
        /// User id.
        /// </summary>
        public virtual long UserId { get; set; }

        /// <summary>
        /// Role id.
        /// </summary>
        public virtual long RoleId { get; set; }

        /// <summary>
        /// Creates a new <see cref="MUserRole"/> object.
        /// </summary>
        public MUserRole()
        {
        }

        /// <summary>
        /// Creates a new <see cref="MUserRole"/> object.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="roleId">Role id</param>
        public MUserRole(long userId, long roleId)
        {
            UserId = userId;
            RoleId = (int)roleId;
        }
    }
}