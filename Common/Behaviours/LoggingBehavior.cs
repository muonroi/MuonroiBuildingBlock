namespace MuonroiBuildingBlock.Common.Behaviours
{
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Stopwatch watch = new();
            watch.Start();
            _logger.LogInformation("----- Handling command {CommandName} ({@Command})", request.GetGenericTypeName(), request);
            TResponse response = await next();
            watch.Stop();
            _logger.LogInformation("----- Handled Command {CommandName} - response ({ExeTime} ms): {@Response}", request.GetGenericTypeName(), watch.ElapsedMilliseconds, response);
            return response;
        }
    }
}