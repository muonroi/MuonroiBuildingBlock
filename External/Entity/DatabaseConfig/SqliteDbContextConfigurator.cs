namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public class SqliteDbContextConfigurator<T> : IDbContextConfigurator<T> where T : MDbContext
    {
        public void Configure(DbContextOptionsBuilder<T> options, string connectionString)
        {
            _ = options.UseSqlite(connectionString);
        }
    }
}