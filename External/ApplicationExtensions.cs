namespace Muonroi.BuildingBlock.External
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
        {
            // Add HttpContextAccessor for accessing the current HTTP context
            _ = services.AddHttpContextAccessor();

            // Register MediatR for handling requests and notifications
            if (assemblies?.Length > 0)
            {
                _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
            }

            // Add ProblemDetails for standardized API error responses
            _ = services.AddProblemDetails();

            // Configure JSON serialization options globally
            _ = services.AddConfigureHttpJson();

            // Register singleton service for date-time management
            _ = services.AddSingleton<IMDateTimeService, MDateTimeService>();

            // Configure controllers with the first available assembly, if any
            Assembly? firstAssembly = assemblies?.FirstOrDefault();
            if (firstAssembly != null)
            {
                _ = services.AddControllerConfiguration(firstAssembly);
            }

            return services;
        }

        public static IServiceCollection SwaggerConfig(this IServiceCollection services, string serviceName)
        {
            // Configure Swagger for API documentation
            _ = services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = serviceName, Version = "v1" });

                // Add Bearer token authentication scheme to Swagger
                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                // Apply Bearer token authentication globally in Swagger
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
            // Initialize localization with required services
            IMJsonSerializeService jsonSerializeService = app.Services.GetRequiredService<IMJsonSerializeService>();
            ResourceSetting resourceSetting = app.Services.GetRequiredService<ResourceSetting>();
            MHelpers.Initialize(resourceSetting, jsonSerializeService, assembly);

            return app;
        }

        public static WebApplicationBuilder AddAppConfigurations(this WebApplicationBuilder builder)
        {
            // Load configuration files based on the environment
            _ = builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();

            return builder;
        }
    }
}