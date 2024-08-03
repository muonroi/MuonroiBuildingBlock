namespace MBuildingBlock.External.DI
{
    public static class ConfigurationDepedencyInjection
    {
        public static IServiceCollection AddScopeServices(this IServiceCollection services, Assembly assembly)
        {
            return services.ResolveDepedencyScope(assembly);
        }
    }
}