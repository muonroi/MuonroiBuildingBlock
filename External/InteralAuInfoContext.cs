namespace Muonroi.BuildingBlock.External
{
    public partial class MAuthenticateInfoContext
    {
        public MAuthenticateInfoContext(int currentUserId, string currentUserGuid, string currentUsername, string accessToken, string apiKey)
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

                if (claims.Exists((m) => m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                {
                    Roles = (from m in claims
                             where m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                             select m.Value).ToList();
                }
            }
        }

        internal MAuthenticateInfoContext(IHttpContextAccessor httpContextAccessor, ResourceSetting resourceSetting)
        {
            string authorizationHeader = "Authorization";
            string? token = httpContextAccessor.HttpContext?.Request.Headers[authorizationHeader];
            string? language = httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString().Split(',').FirstOrDefault();
            resourceSetting["lang"] = language!;
            if (!string.IsNullOrEmpty(token))
            {
                AccessToken = token;
                JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
                JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(token.Replace("Bearer ", ""));
                List<Claim> claims = jwtSecurityToken.Claims.ToList();

                CurrentUserId = GetClaimValue<int>(claims, "user_id");
                CurrentUserGuid = GetClaimValue<string>(claims, "user_guid") ?? string.Empty;
                CurrentUsername = GetClaimValue<string>(claims, "username") ?? string.Empty;

                if (claims.Exists((m) => m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                {
                    Roles = (from m in claims
                             where m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                             select m.Value).ToList();
                }
            }

            ApiKey = httpContextAccessor.HttpContext?.Request.Headers["ApiKey"]!;
        }

        internal MAuthenticateInfoContext(IAmqpContext amqpContext)
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

                if (claims.Exists((m) => m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                {
                    Roles = (from m in claims
                             where m.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                             select m.Value).ToList();
                }
            }
        }

        internal bool HasRole(string roles)
        {
            return !string.IsNullOrEmpty(roles) && Roles != null && Roles.Count != 0
            && Roles.Exists((m) => roles.Split(',').Contains(m));
        }

        internal static T? GetClaimValue<T>(List<Claim> claims, string claimType)
        {
            if (claims.Exists((c) => c.Type == claimType))
            {
                string value = claims.First((c) => c.Type == claimType).Value;
                if (!string.IsNullOrEmpty(value))
                {
                    return (T)global::System.Convert.ChangeType(value, typeof(T));
                }
            }
            return default;
        }
    }
}