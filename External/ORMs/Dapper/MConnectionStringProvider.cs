namespace MBuildingBlock.External.ORMs.Dapper;

public class MConnectionStringProvider(IConfiguration configuration) : IConnectionStringProvider
{
    public string GetConnectionString(string connectionName, bool enableMasterSlave = false, bool readOnly = false)
    {
        string? secretKey = configuration.GetCryptConfigValue("SecretKey");
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("SecretKey cannot be an empty string");
        }
        string configKey = $"{connectionName}:ConnectionString";
        return configuration.GetCryptConfigValue(configKey, secretKey)!;
    }
}