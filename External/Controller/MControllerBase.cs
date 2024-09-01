namespace Muonroi.BuildingBlock.External.Controller
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class MControllerBase(IMediator mediator, ILogger logger) : ControllerBase
    {
        protected IMediator Mediator { get; } = mediator;

        protected ILogger Logger { get; } = logger;

        protected async Task<IActionResult> SendAsync<TRequest, TResponse>(TRequest request)
            where TRequest : IRequest<TResponse>
        {
            TResponse? response = await Mediator.Send(request);
            return Ok(response);
        }

        protected async Task<IActionResult> PublishAsync<TNotification>(TNotification notification)
            where TNotification : INotification
        {
            await Mediator.Publish(notification);
            return Ok();
        }

        protected async Task<IActionResult> SendAsync<TRequest>(TRequest request)
            where TRequest : IRequest
        {
            await Mediator.Send(request);
            return Ok();
        }

        protected async Task<IActionResult> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IRequest
        {
            await Mediator.Send(request, cancellationToken);
            return Ok();
        }

        protected async Task<IActionResult> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
        {
            TResponse? response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        protected async Task<IActionResult> PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
            where TNotification : INotification
        {
            await Mediator.Publish(notification, cancellationToken);
            return Ok();
        }
    }
}