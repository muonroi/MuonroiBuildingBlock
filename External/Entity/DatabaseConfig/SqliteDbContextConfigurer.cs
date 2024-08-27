namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public class SqliteDbContextConfigurer<T> : IDbContextConfigurer<T> where T : MDbContext
    {
        public void Configure(DbContextOptionsBuilder<T> options, string connectionString)
        {
            _ = options.UseSqlite(connectionString);
        }
    }
}