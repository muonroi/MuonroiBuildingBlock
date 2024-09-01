namespace Muonroi.BuildingBlock.External.Entity
{
    public static class MDbContextConfiguration
    {
        public static IServiceCollection AddDbContextConfigure<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isSecrectDefault = true,
            string serectKey = "")
            where T : MDbContext
        {
            // Lấy cấu hình cơ sở dữ liệu từ cấu hình
            DatabaseConfigs? databaseConfigs = configuration.GetSection(nameof(DatabaseConfigs)).Get<DatabaseConfigs>();
            if (databaseConfigs == null || string.IsNullOrEmpty(databaseConfigs.DbType))
            {
                throw new InvalidDataException("Database configuration is not properly set.");
            }

            // Cấu hình cho MongoDB
            if (databaseConfigs.DbType == nameof(DbTypes.MongoDb))
            {
                MongoDbContextConfigurer<T> mongoConfigurer = new();
                _ = mongoConfigurer.ConfigureMongoDb(services, configuration);
            }
            else
            {
                // Giải mã chuỗi kết nối và đăng ký DbContext
                string connectionString = DecryptConnectionString(databaseConfigs, configuration, isSecrectDefault, serectKey);
                ConfigureDbContext<T>(services, databaseConfigs.DbType, connectionString);
            }

            return services;
        }

        private static string DecryptConnectionString(DatabaseConfigs databaseConfigs, IConfiguration configuration, bool isSecrectDefault, string serectkey)
        {
            return databaseConfigs.DbType switch
            {
                nameof(DbTypes.SqlServer) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.SqlServerConnectionString, isSecrectDefault, serectkey),
                nameof(DbTypes.MySql) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.MySqlConnectionString, isSecrectDefault, serectkey),
                nameof(DbTypes.PostgreSql) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.PostgreSqlConnectionString, isSecrectDefault, serectkey),
                nameof(DbTypes.Sqlite) => MStringExtention.DecryptConfigurationValue(configuration, databaseConfigs.ConnectionStrings?.SqliteConnectionString, isSecrectDefault, serectkey),
                _ => throw new ArgumentException("Unsupported database type: " + databaseConfigs.DbType),
            } ?? throw new InvalidDataException("Connection string is not provided or is empty.");
        }

        private static void ConfigureDbContext<T>(IServiceCollection services, string dbType, string connectionString)
            where T : MDbContext
        {
            // Đăng ký cấu hình DbContext tương ứng
            _ = dbType switch
            {
                nameof(DbTypes.SqlServer) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(SqlServerDbContextConfigurer<T>)),
                nameof(DbTypes.MySql) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(MySqlDbContextConfigurer<T>)),
                nameof(DbTypes.PostgreSql) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(PostgreSqlDbContextConfigurer<T>)),
                nameof(DbTypes.Sqlite) => services.AddSingleton(typeof(IDbContextConfigurer<T>), typeof(SqliteDbContextConfigurer<T>)),
                _ => throw new ArgumentException("Unsupported database type: " + dbType),
            };

            // Cấu hình DbContext với chuỗi kết nối đã giải mã
            _ = services.AddDbContext<T>((serviceProvider, options) =>
            {
                IDbContextConfigurer<T> configurer = serviceProvider.GetRequiredService<IDbContextConfigurer<T>>();
                configurer.Configure((DbContextOptionsBuilder<T>)options, connectionString);
            });
        }
    }
}