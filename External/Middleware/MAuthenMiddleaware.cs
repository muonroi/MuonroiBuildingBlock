

namespace Muonroi.BuildingBlock.External.Middleware
{
    public class MAuthenMiddleware<TDbContext>(TDbContext dbContext, RequestDelegate next)
        where TDbContext : MDbContext
    {
        private readonly RequestDelegate _next = next;

        private readonly TDbContext _dbContext = dbContext;

        public async Task InvokeAsync(HttpContext context)
        {
            string authorizationHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authorizationHeader) && !authorizationHeader.StartsWith("Bearer "))
            {
                context.Request.Headers.Authorization = $"Bearer {authorizationHeader}";

                await AuthorizeInternal<TDbContext>.ResolveTokenValidityKey(authorizationHeader, _dbContext, context);
            }
            await _next(context);
        }
    }
}