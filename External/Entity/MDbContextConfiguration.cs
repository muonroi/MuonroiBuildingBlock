

namespace Muonroi.BuildingBlock.External.Entity
{
    public static class MDbContextConfiguration
    {
        public static IServiceCollection AddDbContextConfigure<TDbContext, TPermission>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isSecretDefault = true,
            string secretKey = "")
            where TDbContext : MDbContext
            where TPermission : Enum
        {
            DatabaseConfigs? databaseConfigs = configuration.GetSection(nameof(DatabaseConfigs)).Get<DatabaseConfigs>();
            if (databaseConfigs == null || string.IsNullOrEmpty(databaseConfigs.DbType))
            {
                throw new InvalidDataException("Database configuration is not properly set.");
            }

            if (databaseConfigs.DbType == nameof(DbTypes.MongoDb))
            {
                MongoDbContextConfigurator<TDbContext> mongoConfigurator = new();
                _ = mongoConfigurator.ConfigureMongoDb(services, configuration);
            }
            else
            {
                string connectionString = DecryptConnectionString(databaseConfigs, configuration, isSecretDefault, secretKey);
                ConfigureDbContext<TDbContext>(services, databaseConfigs.DbType, connectionString);
            }
            _ = services.AddScoped(typeof(IPermissionSyncService), typeof(PermissionSyncService<TDbContext>));

            services.SystemDependencyInjectionService<TDbContext, TPermission>();

            return services;
        }

        private static void SystemDependencyInjectionService<TDbContext, TPermission>(this IServiceCollection services)
            where TDbContext : MDbContext
            where TPermission : Enum
        {
            _ = services.AddScoped<IAuthenticateRepository, AuthenticateRepository<TDbContext, TPermission>>();
        }


        private static string DecryptConnectionString(DatabaseConfigs databaseConfigs, IConfiguration configuration, bool isSecretDefault, string secretKey)
        {
            return databaseConfigs.DbType switch
            {
                nameof(DbTypes.SqlServer) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.SqlServerConnectionString, isSecretDefault, secretKey),
                nameof(DbTypes.MySql) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.MySqlConnectionString, isSecretDefault, secretKey),
                nameof(DbTypes.PostgreSql) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.PostgreSqlConnectionString, isSecretDefault, secretKey),
                nameof(DbTypes.Sqlite) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.SqliteConnectionString, isSecretDefault, secretKey),
                _ => throw new ArgumentException("Unsupported database type: " + databaseConfigs.DbType),
            } ?? throw new InvalidDataException("Connection string is not provided or is empty.");
        }

        private static void ConfigureDbContext<T>(IServiceCollection services, string dbType, string connectionString)
            where T : MDbContext
        {
            _ = dbType switch
            {
                nameof(DbTypes.SqlServer) => services.AddSingleton(typeof(IDbContextConfigurator<T>), typeof(SqlServerDbContextConfigurator<T>)),
                nameof(DbTypes.MySql) => services.AddSingleton(typeof(IDbContextConfigurator<T>), typeof(MySqlDbContextConfigurator<T>)),
                nameof(DbTypes.PostgreSql) => services.AddSingleton(typeof(IDbContextConfigurator<T>), typeof(PostgreSqlDbContextConfigurator<T>)),
                nameof(DbTypes.Sqlite) => services.AddSingleton(typeof(IDbContextConfigurator<T>), typeof(SqliteDbContextConfigurator<T>)),
                _ => throw new ArgumentException("Unsupported database type: " + dbType),
            };

            _ = services.AddDbContext<T>((serviceProvider, options) =>
            {
                IDbContextConfigurator<T> Configurator = serviceProvider.GetRequiredService<IDbContextConfigurator<T>>();
                Configurator.Configure((DbContextOptionsBuilder<T>)options, connectionString);
            });
        }
    }
}