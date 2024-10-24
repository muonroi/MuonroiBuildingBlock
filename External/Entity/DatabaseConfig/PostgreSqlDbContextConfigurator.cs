namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public class PostgreSqlDbContextConfigurator<T> : IDbContextConfigurator<T> where T : MDbContext
    {
        public void Configure(DbContextOptionsBuilder<T> options, string connectionString)
        {
            _ = options.UseNpgsql(connectionString);
        }
    }
}