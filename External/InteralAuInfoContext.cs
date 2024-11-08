

namespace Muonroi.BuildingBlock.External
{
    public partial class MAuthenticateInfoContext
    {
        internal MAuthenticateInfoContext(IHttpContextAccessor httpContextAccessor,
            ResourceSetting resourceSetting,
            IConfiguration configuration)
        {
            HttpContext? context = httpContextAccessor.HttpContext;

            if (context == null)
            {
                return;
            }

            IsAuthenticated = context.Items[nameof(IsAuthenticated)] is bool isAuthenticated && isAuthenticated;

            CorrelationId = context.Request.Headers[CustomHeader.CorrelationId].FirstOrDefault() ?? Guid.NewGuid().ToString();
            AccessToken = context.Request.Headers.Authorization;
            Language = context.Request.Headers.AcceptLanguage.ToString().Split(',').FirstOrDefault() ?? "vi-VN";
            resourceSetting[ClaimConstants.UserIdentifier] = Language;

            if (!string.IsNullOrEmpty(AccessToken))
            {
                ExtractRolesFromToken(AccessToken);

                List<Claim> claims = ExtractClaimsFromToken(AccessToken);
                CurrentUserGuid = GetClaimValue<string>(claims, ClaimConstants.UserIdentifier) ?? string.Empty;
                TokenValidityKey = GetClaimValue<string>(claims, ClaimConstants.TokenValidityKey) ?? string.Empty;
                CurrentUsername = GetClaimValue<string>(claims, ClaimConstants.Username) ?? string.Empty;
            }
            ApiKey = configuration.GetConfigHelper(ClaimConstants.ApiKey);
        }

        internal MAuthenticateInfoContext(IAmqpContext amqpContext)
        {
            CorrelationId = amqpContext.GetHeaderByKey(CustomHeader.CorrelationId) ?? Guid.NewGuid().ToString();
            CurrentUserGuid = amqpContext.GetHeaderByKey(ClaimConstants.UserIdentifier) ?? string.Empty;
            CurrentUsername = amqpContext.GetHeaderByKey(ClaimConstants.Username) ?? string.Empty;
            AccessToken = amqpContext.GetHeaderByKey(ClaimConstants.AccessToken);
            if (!string.IsNullOrEmpty(AccessToken))
            {
                ExtractRolesFromToken(AccessToken);
            }
        }

        internal static T? GetClaimValue<T>(List<Claim> claims, string claimType)
        {
            Claim? claim = claims.FirstOrDefault(c => c.Type == claimType);
            return claim != null && !string.IsNullOrEmpty(claim.Value) ? (T)Convert.ChangeType(claim.Value, typeof(T)) : default;
        }

        private void ExtractRolesFromToken(string token)
        {
            List<Claim> claims = ExtractClaimsFromToken(token);
            Permission = claims.FirstOrDefault(m => m.Type == ClaimConstants.Permission)?.Value;
        }

        private static List<Claim> ExtractClaimsFromToken(string token)
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));
            return jwtToken.Claims.ToList();
        }
    }
}