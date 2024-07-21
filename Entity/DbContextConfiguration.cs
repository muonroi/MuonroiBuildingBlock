namespace MuonroiBuildingBlock.Entity
{
    public static class DbContextConfiguration
    {
        public static IServiceCollection AddDbContextConfigure<T>(this IServiceCollection services, IConfiguration configuration)
            where T : BaseDbContext
        {
            _ = services.AddDbContext<T>(options =>
                 options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}