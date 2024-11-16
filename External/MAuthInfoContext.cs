namespace Muonroi.BuildingBlock.External
{
    public partial class MAuthenticateInfoContext
    {
        public MAuthenticateInfoContext(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
        }
        public string CorrelationId { get; set; } = string.Empty;
        public string CurrentUserGuid { get; set; } = string.Empty;
        public string CurrentUsername { get; set; } = string.Empty;
        public string TokenValidityKey { get; set; } = string.Empty;
        public string? AccessToken { get; set; } = string.Empty;
        public string? ApiKey { get; set; } = string.Empty;
        public string? Permission { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Caller { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }

        public string GetAccessToken()
        {
            return AccessToken ?? string.Empty;
        }
    }
}