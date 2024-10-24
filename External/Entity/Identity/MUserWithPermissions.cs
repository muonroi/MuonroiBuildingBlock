namespace Muonroi.BuildingBlock.External.Entity.Identity
{
    public class MUserWithPermissions : MUser
    {
        public List<MPermission> Permissions { get; set; } = [];

        public bool HasPermission<TPermission>(TPermission requiredPermission) where TPermission : Enum
        {
            long requiredPermissionValue = Convert.ToInt64(requiredPermission);

            foreach (MPermission permission in Permissions)
            {
                if ((Convert.ToInt64(permission.Name) & requiredPermissionValue) == requiredPermissionValue)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
