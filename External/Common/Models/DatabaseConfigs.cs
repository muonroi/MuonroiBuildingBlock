namespace Muonroi.BuildingBlock.External.Common.Models
{
    public class DatabaseConfigs
    {
        public string DbType { get; set; } = string.Empty;
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public DatabaseSettings DatabaseSettings { get; set; } = new();
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; } = string.Empty;
        public string? MongoDbConnectionString { get; set; }
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}