namespace MBuildingBlock.External
{
    public static class MlicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
        {
            _ = services.AddHttpContextAccessor();
            _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
            _ = services.AddProblemDetails();
            _ = services.AddConfigureHttpJson();
            _ = services.AddSingleton<IMDateTimeService, MDateTimeService>();
            _ = services.AddControllerConfiguration(assemblies.FirstOrDefault()!);
            return services;
        }

        public static IServiceCollection SwaggerConfig(this IServiceCollection services, string serviceName)
        {
            _ = services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = $"{serviceName}", Version = "v1" });
                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                config.AddSecurityRequirement(new OpenApiSecurityRequirement()
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

        public static IServiceCollection AddAuthContext(this IServiceCollection services)
        {
            _ = services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true,
                               reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder;
        }

        public static IServiceCollection AddControllerConfiguration(this IServiceCollection services, Assembly assemblies)
        {
            _ = services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
            _ = services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new MDateTimeConverter());
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