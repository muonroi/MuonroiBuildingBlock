namespace Muonroi.BuildingBlock.External.Middleware
{
    public class MAuthenMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            string authorizationHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authorizationHeader) && !authorizationHeader.StartsWith("Bearer "))
            {
                context.Request.Headers.Authorization = $"Bearer {authorizationHeader}";
            }

            await _next(context);
        }
    }
}