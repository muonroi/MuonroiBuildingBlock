namespace MBuildingBlock.External
{
    public partial class MAuthInfoContext
    {
        public string UserId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public int CurrentUserId { get; set; }
        public string CurrentUserGuid { get; set; } = string.Empty;
        public string? CurrentUsername { get; set; } = string.Empty;
        public string? AccessToken { get; set; } = string.Empty;
        public string? ApiKey { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public string Language { get; set; } = string.Empty;
        public string Caller { get; set; } = string.Empty;
        public string ClientIpAddr { get; set; } = string.Empty;
        public string AgentCode { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }

        public MAuthInfoContext()
        { }
    }
}