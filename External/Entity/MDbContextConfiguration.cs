namespace Muonroi.BuildingBlock.External.Entity
{
    public static class MDbContextConfiguration
    {
        public static IServiceCollection AddDbContextConfigure<T>(
            this IServiceCollection services,
            IConfiguration configuration)
            where T : MDbContext
        {
            DatabaseConfigs? databaseConfigs = configuration.GetSection(nameof(DatabaseConfigs)).Get<DatabaseConfigs>();
            if (databaseConfigs == null || string.IsNullOrEmpty(databaseConfigs.DbType))
            {
                throw new InvalidDataException("Database configuration is not properly set.");
            }

            if (databaseConfigs.DbType == nameof(DbTypes.MongoDb))
            {
                MongoDbContextConfigurer<T> mongoConfigurer = new();
                _ = mongoConfigurer.ConfigureMongoDb(services, configuration);
            }
            else
            {
                string connectionString = databaseConfigs.DbType switch
                {
                    nameof(DbTypes.SqlServer) => databaseConfigs.ConnectionStrings?.SqlServerConnectionString,
                    nameof(DbTypes.MySql) => databaseConfigs.ConnectionStrings?.MySqlConnectionString,
                    nameof(DbTypes.PostgreSql) => databaseConfigs.ConnectionStrings?.PostgreSqlConnectionString,
                    nameof(DbTypes.Sqlite) => databaseConfigs.ConnectionStrings?.SqliteConnectionString,
                    _ => throw new ArgumentException("Unsupported database type: " + databaseConfigs.DbType),
                } ?? throw new InvalidDataException("Connection string is not provided or is empty.");

                _ = databaseConfigs.DbType switch
                {
                    nameof(DbTypes.SqlServer) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(SqlServerDbContextConfigurer<T>)),
                    nameof(DbTypes.MySql) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(MySqlDbContextConfigurer<T>)),
                    nameof(DbTypes.PostgreSql) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(PostgreSqlDbContextConfigurer<T>)),
                    nameof(DbTypes.Sqlite) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(SqliteDbContextConfigurer<T>)),
                    _ => throw new ArgumentException("Unsupported database type: " + databaseConfigs.DbType),
                };

                _ = services.AddDbContext<T>((serviceProvider, options) =>
                {
                    IDbContextConfigurer<T> configurer = serviceProvider.GetRequiredService<IDbContextConfigurer<T>>();
                    configurer.Configure((DbContextOptionsBuilder<T>)options, connectionString);
                });
            }

            return services;
        }
    }
}