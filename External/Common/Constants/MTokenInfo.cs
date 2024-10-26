namespace Muonroi.BuildingBlock.External.Common.Constants
{
    public class MTokenInfo
    {
        public MTokenInfo()
        {
            SectionName = "TokenConfigs";
        }

        public string SectionName { get; set; }
        public virtual string SigningKeys { get; set; } = null!;
        public virtual string Issuer { get; set; } = null!;
        public virtual string Audience { get; set; } = null!;
        public virtual int ExpiryMinutes { get; set; }
        public int RefreshTokenTTL { get; set; }
        public int RefreshTokenEIM { get; set; }

        public virtual string PublicKey { get; set; } = null!;
        public virtual string PrivateKey { get; set; } = null!;
    }
}