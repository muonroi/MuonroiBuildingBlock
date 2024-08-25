namespace Muonroi.BuildingBlock.External.Logging;

public static class MSerilogAction
{
    public static Action<HostBuilderContext, LoggerConfiguration> Configure => (context, configuration) =>
    {
        _ = configuration
            .ReadFrom.Configuration(context.Configuration);
    };
}