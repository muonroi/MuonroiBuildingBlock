namespace Muonroi.BuildingBlock.External.Logging;

public static class MSerilogAction
{
    public static Action<HostBuilderContext, IServiceProvider, LoggerConfiguration, bool> Configure => (context, services, configuration, enableSelfLog) =>
    {
        _ = configuration
             .ReadFrom.Configuration(context.Configuration)
             .ReadFrom.Services(services)
             .Enrich.FromLogContext();

        if (enableSelfLog)
        {
            SelfLog.Enable(msg => Console.Error.WriteLine(msg));
        }
    };
}