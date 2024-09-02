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
                ExtractRolesFromToken(AccessToken);
            }
        }

        internal MAuthenticateInfoContext(IHttpContextAccessor httpContextAccessor, ResourceSetting resourceSetting)
        {
            HttpContext? context = httpContextAccessor.HttpContext;

            if (context == null)
            {
                return;
            }

            CorrelationId = context.Request.Headers[CustomHeader.CorrelationId].FirstOrDefault() ?? Guid.NewGuid().ToString();
            AccessToken = context.Request.Headers.Authorization;
            Language = context.Request.Headers.AcceptLanguage.ToString().Split(',').FirstOrDefault() ?? "en-US";
            resourceSetting["lang"] = Language;

            if (!string.IsNullOrEmpty(AccessToken))
            {
                ExtractRolesFromToken(AccessToken);

                List<Claim> claims = ExtractClaimsFromToken(AccessToken);
                CurrentUserId = GetClaimValue<int>(claims, "user_id");
                CurrentUserGuid = GetClaimValue<string>(claims, "user_guid") ?? string.Empty;
                CurrentUsername = GetClaimValue<string>(claims, "username") ?? string.Empty;
            }

            ApiKey = context.Request.Headers[CustomHeader.ApiKey];
        }

        internal MAuthenticateInfoContext(IAmqpContext amqpContext)
        {
            CorrelationId = amqpContext.GetHeaderByKey(CustomHeader.CorrelationId) ?? Guid.NewGuid().ToString();
            _ = int.TryParse(amqpContext.GetHeaderByKey("user_id"), out int result);
            CurrentUserId = result;

            CurrentUserGuid = amqpContext.GetHeaderByKey("user_guid") ?? string.Empty;
            CurrentUsername = amqpContext.GetHeaderByKey("username");
            AccessToken = amqpContext.GetHeaderByKey("access_token");
            ApiKey = amqpContext.GetHeaderByKey(CustomHeader.ApiKey);

            if (!string.IsNullOrEmpty(AccessToken))
            {
                ExtractRolesFromToken(AccessToken);
            }
        }

        internal bool HasRole(string roles)
        {
            if (string.IsNullOrEmpty(roles) || Roles == null || Roles.Count == 0)
            {
                return false;
            }

            string[] roleList = roles.Split(',');
            return Roles.Intersect(roleList).Any();
        }

        internal static T? GetClaimValue<T>(List<Claim> claims, string claimType)
        {
            Claim? claim = claims.FirstOrDefault(c => c.Type == claimType);
            return claim != null && !string.IsNullOrEmpty(claim.Value) ? (T)Convert.ChangeType(claim.Value, typeof(T)) : default;
        }

        private void ExtractRolesFromToken(string token)
        {
            List<Claim> claims = ExtractClaimsFromToken(token);
            Roles = claims.Where(m => m.Type == ClaimTypes.Role)
                          .Select(m => m.Value)
                          .ToList();
        }

        private static List<Claim> ExtractClaimsFromToken(string token)
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));
            return jwtToken.Claims.ToList();
        }
    }
}