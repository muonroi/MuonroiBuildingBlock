namespace Muonroi.BuildingBlock.External.Entity
{
    public static class MDbContextConfiguration
    {
        public static IServiceCollection AddDbContextConfigure<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectName = "DefaultConnection",
            string dbType = nameof(DbTypes.SqlServer))
            where T : MDbContext
        {
            string? connectionString = configuration.GetConnectionString(connectName);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectName), $"Connection string '{connectName}' is not configured.");
            }

            _ = dbType switch
            {
                nameof(DbTypes.SqlServer) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(SqlServerDbContextConfigurer<T>)),
                nameof(DbTypes.MySql) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(MySqlDbContextConfigurer<T>)),
                nameof(DbTypes.PostgreSql) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(PostgreSqlDbContextConfigurer<T>)),
                nameof(DbTypes.Sqlite) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(SqliteDbContextConfigurer<T>)),
                nameof(DbTypes.MongoDb) => throw new ArgumentException("MongoDB does not support DbContextFactory directly."),
                _ => throw new ArgumentException("Unsupported database type: " + dbType),
            };

            _ = services.AddDbContext<T>((serviceProvider, options) =>
            {
                IDbContextConfigurer<T> configurer = serviceProvider.GetRequiredService<IDbContextConfigurer<T>>();
                configurer.Configure((DbContextOptionsBuilder<T>)options, connectionString);
            });

            return services;
        }
    }
}