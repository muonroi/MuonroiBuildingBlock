namespace MuonroiBuildingBlock.Infrastructure.ORMs.Dapper;

public class ConnectionStringProvider(IConfiguration configuration) : IConnectionStringProvider
{
    public string GetConnectionString(string connectionName, bool enableMasterSlave = false, bool readOnly = false)
    {
        string? secretKey = configuration.GetCryptConfigValue("SecretKey");
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidConfigException(nameof(HttpStatusCode.InternalServerError), "SecretKey cannot be an empty string");
        }
        string configKey = $"{connectionName}:ConnectionString";
        return configuration.GetCryptConfigValue(configKey, secretKey)!;
    }
}