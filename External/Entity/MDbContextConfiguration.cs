namespace MBuildingBlock.External.Entity
{
    public static class MDbContextConfiguration
    {
        public static IServiceCollection AddDbContextConfigure<T>(this IServiceCollection services, IConfiguration configuration)
            where T : MDbContext
        {
            _ = services.AddDbContext<T>(options =>
                 options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}