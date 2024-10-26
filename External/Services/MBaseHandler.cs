namespace Muonroi.BuildingBlock.External.Services
{
    public class MBaseHandler(ILogger logger,
        MAuthenticateInfoContext authContext,
        IMJsonSerializeService jsonSerialize,
        IMDateTimeService dateTimeService)
    {
        protected ILogger Logger => logger;

        protected IMJsonSerializeService JsonSerialize => jsonSerialize;

        protected IMDateTimeService DateTimeService => dateTimeService;

        protected MAuthenticateInfoContext AuthContext => authContext;

        protected string CurrentUserGuid => AuthContext.CurrentUserGuid;

        protected string? CurrentUsername => AuthContext.CurrentUsername;

        protected void LogInformation(string message, params object[] args)
        {
            Logger.Information(message, args);
        }

        protected void LogWarning(string message, params object[] args)
        {
            Logger.Warning(message, args);
        }

        protected void LogError(string message, params object[] args)
        {
            Logger.Error(message, args);
        }

        protected void LogDebug(string message, params object[] args)
        {
            Logger.Debug(message, args);
        }
    }
}