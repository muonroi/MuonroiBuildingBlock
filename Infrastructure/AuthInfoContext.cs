namespace MuonroiBuildingBlock.Infrastructure
{
    public class AuthInfoContext
    {
        // Thuộc tính người dùng
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

        public AuthInfoContext()
        { }

        public AuthInfoContext(int currentUserId, string currentUserGuid, string currentUsername, string accessToken, string apiKey)
        {
            CurrentUserId = currentUserId;
            CurrentUserGuid = currentUserGuid;
            CurrentUsername = currentUsername;
            AccessToken = accessToken;
            ApiKey = apiKey;

            if (!string.IsNullOrEmpty(AccessToken))
            {
                JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
                JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(AccessToken.Replace("Bearer ", ""));
                List<Claim> claims = jwtSecurityToken.Claims.ToList();

                if (claims.Exists((Claim m) => m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                {
                    Roles = (from m in claims
                             where m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                             select m.Value).ToList();
                }
            }
        }

        public AuthInfoContext(IHttpContextAccessor httpContextAccessor)
        {
            string authorizationHeader = "Authorization";
            string? token = httpContextAccessor.HttpContext?.Request.Headers[authorizationHeader];
            if (!string.IsNullOrEmpty(token))
            {
                AccessToken = token;
                JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
                JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(token.Replace("Bearer ", ""));
                List<Claim> claims = jwtSecurityToken.Claims.ToList();

                CurrentUserId = GetClaimValue<int>(claims, "user_id");
                CurrentUserGuid = GetClaimValue<string>(claims, "user_guid") ?? string.Empty;
                CurrentUsername = GetClaimValue<string>(claims, "username") ?? string.Empty;

                if (claims.Exists((Claim m) => m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                {
                    Roles = (from m in claims
                             where m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                             select m.Value).ToList();
                }
            }

            ApiKey = httpContextAccessor.HttpContext?.Request.Headers["ApiKey"]!;
        }

        // Phương thức khởi tạo với IAmqpContext
        public AuthInfoContext(IAmqpContext amqpContext)
        {
            _ = int.TryParse(amqpContext.GetHeaderByKey("user_id"), out int result);
            CurrentUserId = result;

            string? headerByKey = amqpContext.GetHeaderByKey("user_guid");
            if (!string.IsNullOrEmpty(headerByKey))
            {
                CurrentUserGuid = headerByKey;
            }

            CurrentUsername = amqpContext.GetHeaderByKey("username");

            AccessToken = amqpContext.GetHeaderByKey("access_token");
            ApiKey = amqpContext.GetHeaderByKey("ApiKey");

            if (!string.IsNullOrEmpty(AccessToken))
            {
                JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
                JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(AccessToken.Replace("Bearer ", ""));
                List<Claim> claims = jwtSecurityToken.Claims.ToList();

                if (claims.Exists((Claim m) => m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                {
                    Roles = (from m in claims
                             where m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                             select m.Value).ToList();
                }
            }
        }

        public bool HasRole(string roles)
        {
            return !string.IsNullOrEmpty(roles) && Roles != null && Roles.Count != 0
            && Roles.Exists((string m) => roles.Split(',').Contains(m));
        }

        // Phương thức lấy giá trị của claim
        private static T? GetClaimValue<T>(List<Claim> claims, string claimType)
        {
            if (claims.Exists((Claim c) => c.Type == claimType))
            {
                string value = claims.First((Claim c) => c.Type == claimType).Value;
                if (!string.IsNullOrEmpty(value))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            return default;
        }
    }
}