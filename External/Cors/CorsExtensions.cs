namespace Muonroi.BuildingBlock.External.Cors;

public static class CorsExtensions
{
    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration, string domainName = "MAllowDomains")
    {
        string[] origins = configuration[domainName]?.Trim().Split(",") ?? [];

        _ = services.AddCors(options =>
        {
            options.AddPolicy(domainName, policy =>
            {
                _ = policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}