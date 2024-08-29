namespace Muonroi.BuildingBlock.External.Entity
{
    public class SharedDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : MDbContext
    {
        public TContext CreateDbContext(string[] args)
        {
            string? environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            DatabaseConfigs? databaseConfigs = configuration.GetSection("DatabaseConfigs").Get<DatabaseConfigs>();
            if (databaseConfigs == null || string.IsNullOrEmpty(databaseConfigs.DbType))
            {
                throw new ArgumentNullException(nameof(databaseConfigs), "Database configuration is not properly set.");
            }

            DbContextOptionsBuilder<TContext> builder = new();
            string connectionString = databaseConfigs.DbType switch
            {
                nameof(DbTypes.SqlServer) => databaseConfigs.ConnectionStrings?.SqlServerConnectionString,
                nameof(DbTypes.MySql) => databaseConfigs.ConnectionStrings?.MySqlConnectionString,
                nameof(DbTypes.PostgreSql) => databaseConfigs.ConnectionStrings?.PostgreSqlConnectionString,
                nameof(DbTypes.Sqlite) => databaseConfigs.ConnectionStrings?.SqliteConnectionString,
                _ => throw new ArgumentException("Unsupported database type: " + databaseConfigs.DbType),
            } ?? throw new ArgumentNullException(nameof(connectionString), "Connection string is not provided or is empty.");

            _ = databaseConfigs.DbType switch
            {
                nameof(DbTypes.SqlServer) => builder.UseSqlServer(connectionString),
                nameof(DbTypes.MySql) => builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
                nameof(DbTypes.PostgreSql) => builder.UseNpgsql(connectionString),
                nameof(DbTypes.Sqlite) => builder.UseSqlite(connectionString),
                _ => throw new ArgumentException("Unsupported database type: " + databaseConfigs.DbType),
            };
            return (TContext)Activator.CreateInstance(typeof(TContext), builder.Options)!;
        }
    }
}