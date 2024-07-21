namespace MuonroiBuildingBlock.Contract.Constants
{
    public static class SystemConfig
    {
        public static IServiceCollection AddSystemConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ResourceSetting appSettings = [];
            configuration.GetSection(nameof(ResourceSetting)).Bind(appSettings);
            _ = services.AddSingleton(appSettings);
            return services;
        }
    }
}