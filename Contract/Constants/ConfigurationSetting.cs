namespace MuonroiBuildingBlock.Contract.Constants
{
    public static class ConfigurationSetting
    {
        public const string ConnectionString = "DatabaseConfigs:DefaultConnection";
        public const string ConnectionMongoDbNameString = "DatabaseConfigs:DatabaseName";
        public const string MinIOAccessKey = "MinIOConfig:accessKey";
        public const string MinIOSerrectKey = "MinIOConfig:secretKey";
        public const string EmailPassword = "SmtpConfig:Password";
        public const string JwtSecrectKey = "JwtBearerConfig:SigningKeys";
        public const string JwtIssuer = "JwtBearerConfig:Issuer";
        public const string JwtAudience = "JwtBearerConfig:Audience";
        public const string PublicKey = "PublicKey";
    }
}