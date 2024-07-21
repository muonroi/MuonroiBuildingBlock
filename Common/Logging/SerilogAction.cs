namespace MuonroiBuildingBlock.Common.Logging;

public static class SerilogAction
{
    public static Action<HostBuilderContext, LoggerConfiguration> Configure => (context, configuration) =>
    {
        string? applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");
        string environmentName = context.HostingEnvironment.EnvironmentName;
        _ = configuration
            .ReadFrom.Configuration(context.Configuration)
            .CreateLogger();
    };
}