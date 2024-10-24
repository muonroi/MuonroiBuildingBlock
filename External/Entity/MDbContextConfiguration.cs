namespace Muonroi.BuildingBlock.External.Entity
{
    public static class MDbContextConfiguration
    {
        public static IServiceCollection AddDbContextConfigure<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isSecretDefault = true,
            string secretKey = "") where T : MDbContext
        {
            DatabaseConfigs? databaseConfigs = configuration.GetSection(nameof(DatabaseConfigs)).Get<DatabaseConfigs>();
            if (databaseConfigs == null || string.IsNullOrEmpty(databaseConfigs.DbType))
            {
                throw new InvalidDataException("Database configuration is not properly set.");
            }

            if (databaseConfigs.DbType == nameof(DbTypes.MongoDb))
            {
                MongoDbContextConfigurator<T> mongoConfigurator = new();
                _ = mongoConfigurator.ConfigureMongoDb(services, configuration);
            }
            else
            {
                string connectionString = DecryptConnectionString(databaseConfigs, configuration, isSecretDefault, secretKey);
                ConfigureDbContext<T>(services, databaseConfigs.DbType, connectionString);
            }

            _ = services.AddScoped(typeof(IPermissionSyncService), typeof(PermissionSyncService<T>));

            return services;
        }


        private static string DecryptConnectionString(DatabaseConfigs databaseConfigs, IConfiguration configuration, bool isSecrectDefault, string secretKey)
        {
            return databaseConfigs.DbType switch
            {
                nameof(DbTypes.SqlServer) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.SqlServerConnectionString, isSecrectDefault, secretKey),
                nameof(DbTypes.MySql) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.MySqlConnectionString, isSecrectDefault, secretKey),
                nameof(DbTypes.PostgreSql) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.PostgreSqlConnectionString, isSecrectDefault, secretKey),
                nameof(DbTypes.Sqlite) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.SqliteConnectionString, isSecrectDefault, secretKey),
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