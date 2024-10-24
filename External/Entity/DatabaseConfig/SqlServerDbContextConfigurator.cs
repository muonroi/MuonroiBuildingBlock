namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public class SqlServerDbContextConfigurator<T> : IDbContextConfigurator<T> where T : MDbContext
    {
        public void Configure(DbContextOptionsBuilder<T> options, string connectionString)
        {
            _ = options.UseSqlServer(connectionString);
        }
    }
}