

namespace Muonroi.BuildingBlock.External
{
    internal abstract class BaseCommandHandler(
        IMapper mapper,
        MAuthenticateInfoContext tokenInfo,
        IAuthenticateRepository authenticateRepository,
        ILogger logger,
        IMediator mediator)
    {
        protected IMapper Mapper => mapper;
        protected IAuthenticateRepository AuthenticateRepository => authenticateRepository;
        protected ILogger Logger => logger;
        protected IMediator Mediator => mediator;

        protected string CurrentUserId => tokenInfo.CurrentUserGuid;
        protected string CurrentUsername => tokenInfo.CurrentUsername;
        protected static double NowTsOnlyDay => Clock.UtcNow.GetTimeStamp();
        protected static double NowTs => Clock.UtcNow.GetTimeStamp(true);
        protected static DateTime LocalNow => Clock.Now;
        protected static DateTime Now => Clock.UtcNow;
        protected static double LocalNowTsOnlyDay => Clock.Now.GetTimeStamp();
        protected static double LocalNowTs => Clock.Now.GetTimeStamp(true);


        protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return await Mediator.Send(request, cancellationToken);
        }
        protected async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken) where TNotification : INotification
        {
            await Mediator.Publish(notification, cancellationToken);
        }

        protected void LogInfo(string message)
        {
            Logger.Information(message);
        }
        protected void LogError(string message)
        {
            Logger.Error(message);
        }
        protected void LogError(Exception ex)
        {
            Logger.Error(ex, ex.Message);
        }
        protected void LogError(string message, Exception ex)
        {
            Logger.Error(ex, message);
        }
        protected void LogWarning(string message)
        {
            Logger.Warning(message);
        }

        protected T Map<T>(object source)
        {
            return Mapper.Map<T>(source);
        }

        protected T Map<T>(object source, T destination)
        {
            return Mapper.Map(source, destination);
        }

    }
}
