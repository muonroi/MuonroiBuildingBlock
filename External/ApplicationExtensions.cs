namespace Muonroi.BuildingBlock.External
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
        {
            _ = services.AddHttpContextAccessor();

            if (assemblies?.Length > 0)
            {
                _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
            }

            _ = services.AddProblemDetails();

            _ = services.AddConfigureHttpJson();

            _ = services.AddSingleton<IMDateTimeService, MDateTimeService>();

            Assembly? firstAssembly = assemblies?.FirstOrDefault();
            if (firstAssembly != null)
            {
                _ = services.AddControllerConfiguration(firstAssembly);
            }

            return services;
        }

        public static IServiceCollection SwaggerConfig(this IServiceCollection services, string serviceName)
        {
            _ = services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = serviceName, Version = "v1" });

                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        public static WebApplication AddLocalization(this WebApplication app, Assembly assembly)
        {
            IMJsonSerializeService jsonSerializeService = app.Services.GetRequiredService<IMJsonSerializeService>();
            ResourceSetting resourceSetting = app.Services.GetRequiredService<ResourceSetting>();
            MHelpers.Initialize(resourceSetting, jsonSerializeService, assembly);

            return app;
        }

        public static WebApplicationBuilder AddAppConfigurations(this WebApplicationBuilder builder)
        {
            _ = builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();

            return builder;
        }
    }
}