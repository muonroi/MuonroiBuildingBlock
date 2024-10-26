namespace Muonroi.BuildingBlock.External.Middleware
{
    public class JwtAdminMiddleware(
        RequestDelegate next,
        Func<IServiceProvider, HttpContext, Task<MAuthenticateInfoContext>> callbackVerifyToken)
    {
        private readonly RequestDelegate _next = next;
        private readonly Func<IServiceProvider, HttpContext, Task<MAuthenticateInfoContext>> _callbackVerifyToken = callbackVerifyToken;

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
            IHeaderDictionary headers = context.Request.Headers;

            if (IsAllowAnonymous(context))
            {
                AddHeader(headers, nameof(MAuthenticateInfoContext.CorrelationId), Guid.NewGuid().ToString());

                await _next(context);

                return;
            }

            MAuthenticateInfoContext verifyToken = await _callbackVerifyToken(serviceProvider, context);

            if (!verifyToken.IsAuthenticated)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            _ = headers.TryGetValue("Accept-Language", out StringValues language);
            _ = headers.TryGetValue(nameof(MAuthenticateInfoContext.CorrelationId), out StringValues correlationIds);
            AddHeader(headers, nameof(MAuthenticateInfoContext.CurrentUserGuid), verifyToken.CurrentUserGuid);
            AddHeader(headers, nameof(MAuthenticateInfoContext.TokenValidityKey), verifyToken.TokenValidityKey);
            AddHeader(headers, nameof(MAuthenticateInfoContext.CurrentUsername), verifyToken.CurrentUsername);
            AddHeader(headers, nameof(MAuthenticateInfoContext.Permission), verifyToken.Permission ?? string.Empty);
            AddHeader(headers, "Accept-Language", language.FirstOrDefault() ?? "vi-VN");
            if (correlationIds.Count == 0)
            {
                AddHeader(headers, nameof(MAuthenticateInfoContext.CorrelationId), Guid.NewGuid().ToString());
            }
            Claim username = new(nameof(MAuthenticateInfoContext.CurrentUsername), verifyToken.CurrentUsername);
            Claim userIdentifier = new(nameof(MAuthenticateInfoContext.CurrentUserGuid), verifyToken.CurrentUserGuid);
            context.User = new(new ClaimsIdentity([username, userIdentifier], JwtBearerDefaults.AuthenticationScheme));
            await _next(context);
        }

        private static bool IsAllowAnonymous(HttpContext httpContext)
        {
            Endpoint? endpoint = httpContext.GetEndpoint();
            if (endpoint is null)
            {
                return false;
            }

            AllowAnonymousAttribute? endpointMetadata = endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>();
            AuthorizeAttribute? authorize = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();

            return (endpointMetadata is null && authorize is null) || endpointMetadata is not null;
        }

        private static void AddHeader(IHeaderDictionary headers, string key, string value)
        {
            if (headers.ContainsKey(key))
            {
                _ = headers.Remove(key);
            }
            headers.Append(key, value);
        }
    }
}
