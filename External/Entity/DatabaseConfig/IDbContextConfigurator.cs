namespace Muonroi.BuildingBlock.External.Entity.DatabaseConfig
{
    public interface IDbContextConfigurator<T> where T : MDbContext
    {
        void Configure(DbContextOptionsBuilder<T> options, string connectionString);
    }
}