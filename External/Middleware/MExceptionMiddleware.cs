namespace Muonroi.BuildingBlock.External.Middleware
{
    public class MExceptionMiddleware(RequestDelegate next, ISerilogLogger logger, IMJsonSerializeService serializeService)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ISerilogLogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMJsonSerializeService _serializeService = serializeService ?? throw new ArgumentNullException(nameof(serializeService));

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var response = new
            {
                error = new
                {
                    message = MMethodResultHelpers.GetErrorMessage(nameof(SystemEnum.SYS00)),
                    details = ex.Message
                }
            };

            return context.Response.WriteAsync(_serializeService.Serialize(response));
        }
    }
}