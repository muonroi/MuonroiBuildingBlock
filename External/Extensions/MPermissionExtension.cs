


namespace Muonroi.BuildingBlock.External.Extensions;

public static class MPermissionExtension<TPermission> where TPermission : Enum
{
    public static long CalculatePermissionsBitmask(List<TPermission> userPermissions)
    {
        long bitmask = 0;
        foreach (TPermission permission in userPermissions)
        {
            bitmask |= Convert.ToInt64(permission);
        }
        return bitmask;
    }
}
