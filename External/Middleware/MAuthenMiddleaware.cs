namespace Muonroi.BuildingBlock.External.Middleware
{
    public class MAuthenMiddleware<TDbContext, TPermission>(TDbContext dbContext, RequestDelegate next)
        where TDbContext : MDbContext
        where TPermission : Enum
    {
        private readonly RequestDelegate _next = next;

        private readonly TDbContext _dbContext = dbContext;

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsAllowAnonymous(context))
            {
                await _next(context);
                return;
            }

            string authorizationHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authorizationHeader) && !authorizationHeader.StartsWith("Bearer "))
            {
                context.Request.Headers.Authorization = $"Bearer {authorizationHeader}";

                await _dbContext.ResolveTokenValidityKey<TDbContext, TPermission>(authorizationHeader, context);

                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    return;
                }
            }

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
    }
}
