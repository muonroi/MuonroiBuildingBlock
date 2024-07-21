namespace MuonroiBuildingBlock.Application
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
        {
            _ = services.AddHttpContextAccessor();
            _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
            _ = services.AddProblemDetails();
            _ = services.AddConfigureHttpJson();
            _ = services.AddSingleton<IDateTimeService, DateTimeService>();
            _ = services.AddControllerConfiguration(assemblies.FirstOrDefault()!);
            return services;
        }

        public static IServiceCollection AddAuthContext(this IServiceCollection services)
        {
            _ = services.AddSingleton<IAuthContext, AuthContext>();
            return services;
        }

        public static WebApplication AddLocalization(this WebApplication app)
        {
            IJsonSerializeService jsonSerializeService = app.Services.GetRequiredService<IJsonSerializeService>();
            ResourceSetting resourceSetting = app.Services.GetRequiredService<ResourceSetting>();
            Helpers.Initialize(resourceSetting, jsonSerializeService);
            return app;
        }

        public static void AddAppConfigurations(this WebApplicationBuilder builder)
        {
            _ = builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true,
                               reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        public static IServiceCollection AddControllerConfiguration(this IServiceCollection services, Assembly assemblies)
        {
            _ = services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
            _ = services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
                _ = options.Filters.Add(typeof(ValidationFilterAttribute));
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new Infrastructure.JsonConverter.DateTimeConverter());
            }).AddControllersAsServices();

            //This method must be called after AddControllers
            _ = services.AddFluentValidationAutoValidation();
            _ = services.AddValidatorsFromAssembly(assemblies);

            return services;
        }

        public static IServiceCollection AddConfigureHttpJson(this IServiceCollection services)
        {
            _ = services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.WriteIndented = false;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            return services;
        }
    }
}