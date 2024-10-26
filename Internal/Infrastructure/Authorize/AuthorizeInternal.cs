
namespace Muonroi.BuildingBlock.Internal.Infrastructure.Authorize
{
    public static class AuthorizeInternal<TDbContext>
        where TDbContext : MDbContext
    {
        internal static async Task ResolveTokenValidityKey(string authorizationHeader,
           TDbContext dbContext,
           HttpContext context)
        {
            List<Claim> claims = ExtractClaimsFromToken(authorizationHeader);

            string tokenValidity = GetClaimValue<string>(claims, ClaimConstants.TokenValidityKey) ?? string.Empty;

            string userIdentifier = GetClaimValue<string>(claims, ClaimConstants.UserIdentifier) ?? string.Empty;

            MRefreshToken? refresh = await dbContext.RefreshTokens.SingleOrDefaultAsync(x =>
                                            x.TokenValidityKey == tokenValidity);

            if (refresh is null || refresh.IsDeleted || refresh.IsRevoked)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid token.");
                return;
            }
        }
        private static T? GetClaimValue<T>(List<Claim> claims, string claimType)
        {
            Claim? claim = claims.FirstOrDefault(c => c.Type == claimType);
            return claim != null && !string.IsNullOrEmpty(claim.Value) ? (T)Convert.ChangeType(claim.Value, typeof(T)) : default;
        }
        private static List<Claim> ExtractClaimsFromToken(string token)
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token.Replace("Bearer ", ""));
            return jwtToken.Claims.ToList();
        }
    }
}
