namespace MuonroiBuildingBlock.Contract.Constants
{
    public class JwtConfig
    {
        public const string SectionName = "JwtConfigs";
        public string SigningKeys { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpiresInMinutes { get; set; }
    }
}