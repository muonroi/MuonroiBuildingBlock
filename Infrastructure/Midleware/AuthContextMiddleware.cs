namespace MuonroiBuildingBlock.Infrastructure.Midleware;

public class AuthContextMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, ResourceSetting resourceSetting)
    {
        IAuthContext accessor = context.RequestServices.GetRequiredService<IAuthContext>();

        IHeaderDictionary headers = context.Request.Headers;
        _ = headers.TryGetValue(nameof(AuthInfoContext.CorrelationId), out StringValues correlationIds);

        _ = headers.TryGetValue("Accept-Language", out StringValues languages);
        _ = headers.TryGetValue(nameof(AuthInfoContext.Roles), out StringValues roles);
        _ = headers.TryGetValue(nameof(AuthInfoContext.AgentCode), out StringValues agentCodes);
        _ = headers.TryGetValue(nameof(AuthInfoContext.UserId), out StringValues userIds);
        _ = headers.TryGetValue(nameof(AuthInfoContext.Roles), out _);
        _ = headers.TryGetValue(nameof(AuthInfoContext.IsAuthenticated), out StringValues isAuthenticateds);

        string clientIpAddr = context.GetRequestedIpAddress();
        string caller = context.GetHeaderUserAgent();
        string userId = userIds.FirstOrDefault() ?? string.Empty;
        _ = bool.TryParse(isAuthenticateds.FirstOrDefault(), out bool isAuthenticated);
        accessor.AuthInfoContext = new AuthInfoContext
        {
            CorrelationId = correlationIds.FirstOrDefault() ?? Guid.NewGuid().ToString(),
            ClientIpAddr = clientIpAddr,
            Caller = caller,
            Language = languages.FirstOrDefault() ?? "vi-VN",
            Roles = [.. roles.ToString().Split(',')],
            AgentCode = agentCodes.FirstOrDefault() ?? string.Empty,
            UserId = userId,
            IsAuthenticated = isAuthenticated,
        };
        string? language = accessor.AuthInfoContext.Language.Split(',').FirstOrDefault();
        resourceSetting["lang"] = language!;

        await next(context);
    }
}