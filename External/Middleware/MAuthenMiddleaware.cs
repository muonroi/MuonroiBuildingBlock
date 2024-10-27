

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
            string authorizationHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authorizationHeader) && !authorizationHeader.StartsWith("Bearer "))
            {
                context.Request.Headers.Authorization = $"Bearer {authorizationHeader}";

                await _dbContext.ResolveTokenValidityKey<TDbContext, TPermission>(authorizationHeader, context);
            }
            await _next(context);
        }
    }
}