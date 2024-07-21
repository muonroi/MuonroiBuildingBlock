namespace MuonroiBuildingBlock.Infrastructure.Midleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ISerilogLogger logger, IJsonSerializeService serializeService)
{
    private readonly RequestDelegate _next = next;

    private readonly ISerilogLogger _logger = logger;

    private readonly IJsonSerializeService _serializeService = serializeService;

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
                message = MethodResultHelpers.GetErrorMessage(nameof(SystemEnum.SYS00)),
                details = ex.Message
            }
        };
        return context.Response.WriteAsync(_serializeService.Serialize(response));
    }
}