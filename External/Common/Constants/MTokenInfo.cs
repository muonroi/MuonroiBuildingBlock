namespace MBuildingBlock.External.Common.Constants
{
    public class MTokenInfo(string sectionName = "TokenConfigs")
    {
        public string SectionName = sectionName;
        public virtual string SigningKeys { get; set; } = null!;
        public virtual string Issuer { get; set; } = null!;
        public virtual string Audience { get; set; } = null!;
        public virtual int ExpiryMinutes { get; set; }
        public virtual string PublicKey { get; set; } = null!;
        public virtual string PrivateKey { get; set; } = null!;
    }
}