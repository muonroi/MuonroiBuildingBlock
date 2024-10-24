namespace Muonroi.BuildingBlock.External.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute<TPermission>(TPermission requiredPermission) : Attribute where TPermission : Enum
    {
        public TPermission RequiredPermission { get; } = requiredPermission;
    }
}
