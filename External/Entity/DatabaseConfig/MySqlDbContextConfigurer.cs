namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public class MySqlDbContextConfigurer<T> : IDbContextConfigurer<T> where T : MDbContext
    {
        public void Configure(DbContextOptionsBuilder<T> options, string connectionString)
        {
            _ = options.UseMySql(
                            connectionString,
                            ServerVersion.AutoDetect(connectionString));
        }
    }
}