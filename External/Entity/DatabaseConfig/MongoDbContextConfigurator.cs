namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public class MongoDbContextConfigurator<T> : IDbContextConfigurator<T> where T : MDbContext
    {
        public void Configure(DbContextOptionsBuilder<T> options, string connectionString)
        {
            throw new NotSupportedException("MongoDB does not use DbContextOptionsBuilder. Configure MongoDB services directly in the IServiceCollection.");
        }

        public IServiceCollection ConfigureMongoDb(IServiceCollection services, IConfiguration configuration)
        {
            string? mongoDbConnectionString = configuration.GetConnectionString("MongoDbConnectionString");
            if (string.IsNullOrEmpty(mongoDbConnectionString))
            {
                throw new InvalidDataException("MongoDb connection string is not configured.");
            }

            string? mongoDbName = configuration.GetSection("DatabaseConfigs")["DatabaseName"];
            if (string.IsNullOrEmpty(mongoDbName))
            {
                throw new InvalidDataException("MongoDb database name is not configured.");
            }

            string result = $"{mongoDbConnectionString}/{mongoDbName}?authSource=admin";

            _ = services.AddSingleton<IMongoClient>(new MongoClient(result))
                        .AddScoped(x => x.GetService<IMongoClient>()!.StartSession());

            DatabaseConfigs? databaseSettings = configuration.GetSection(nameof(DatabaseConfigs)).Get<DatabaseConfigs>();
            _ = services.AddSingleton(databaseSettings!);

            return services;
        }
    }
}