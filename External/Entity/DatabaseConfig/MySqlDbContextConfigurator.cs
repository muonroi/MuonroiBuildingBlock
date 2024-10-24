namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public class MySqlDbContextConfigurator<T> : IDbContextConfigurator<T> where T : MDbContext
    {
        public void Configure(DbContextOptionsBuilder<T> options, string connectionString)
        {
            _ = options.UseMySql(
                            connectionString,
                            Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connectionString));
        }
    }
}