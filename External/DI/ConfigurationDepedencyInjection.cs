namespace Muonroi.BuildingBlock.External.DI
{
    public static class ConfigurationDependencyInjection
    {
        public static IServiceCollection AddScopeServices(this IServiceCollection services, Assembly assembly)
        {
            return services.ResolveDependencyScope(assembly);
        }
    }
}