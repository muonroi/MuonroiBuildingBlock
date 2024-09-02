namespace Muonroi.BuildingBlock.External
{
    public static class InfrastructureExtensions
    {
        public static readonly Assembly? EntryAssembly = Assembly.GetEntryAssembly();

        public static IServiceCollection AddInfrastructure<TProgram>(
                this IServiceCollection services,
                IConfiguration configuration,
                MTokenInfo? tokenConfig = null,
                MPaginationConfig? paginationConfigs = null,
                bool isSecrectDefault = true,
                string serectKey = "")
        {
            _ = services.AddControllersWithOptions()
                    .AddApiDocumentation<TProgram>()
                    .AddCoreServices(configuration, isSecrectDefault, serectKey, paginationConfigs, tokenConfig)
                    .AddAuthContext();

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            RedisConfigs redisConfigs = serviceProvider.GetRequiredService<RedisConfigs>();
            _ = services.AddRedis(configuration, redisConfigs)
                    .AddDapperCaching(configuration, redisConfigs);

            return services;
        }

        private static IServiceCollection AddControllersWithOptions(this IServiceCollection services)
        {
            _ = services.AddControllers(options =>
            {
                _ = options.Filters.Add<DefaultProducesResponseTypeFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            })
            .AddControllersAsServices();

            return services;
        }

        private static IServiceCollection AddApiDocumentation<TProgram>(this IServiceCollection services)
        {
            _ = services.AddEndpointsApiExplorer()
                .AddSwaggerGen(options =>
                {
                    options.OperationFilter<SwaggerDefaultValues>();
                    string fileName = typeof(TProgram).Assembly.GetName().Name + ".xml";
                    string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                    options.IncludeXmlComments(filePath);
                });

            return services;
        }

        private static IServiceCollection AddCoreServices(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isSecrectDefault,
            string serectKey,
            MPaginationConfig? paginationConfigs,
            MTokenInfo? tokenConfig)
        {
            _ = services.AddHttpContextAccessor()
                .AddProblemDetails()
                .AddSingleton<IMJsonSerializeService, MJsonSerializeService>()
                .AddSingleton<IMDateTimeService, MDateTimeService>()
                .AddSingleton<ICacheProvider, RedisCacheProvider>()
                .AddRedisConfiguration(configuration, isSecrectDefault, serectKey)
                .AddPaginationConfigs(configuration, paginationConfigs)
                .AddJwtConfigs(configuration, tokenConfig)
                .AddSystemConfig(configuration);

            return services;
        }

        public static IServiceCollection AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration,
        bool isSecrectDefault = true,
        string serectKey = "")
        {
            RedisConfigs redisConfigs = new();
            configuration.GetSection(redisConfigs.SectionName).Bind(redisConfigs);

            redisConfigs.Host = MStringExtention.DecryptConfigurationValue(configuration, redisConfigs.Host, isSecrectDefault, serectKey) ?? throw new InvalidDataException();
            redisConfigs.Port = MStringExtention.DecryptConfigurationValue(configuration, redisConfigs.Port, isSecrectDefault, serectKey) ?? throw new InvalidDataException(); ;
            redisConfigs.Password = MStringExtention.DecryptConfigurationValue(configuration, redisConfigs.Password, isSecrectDefault, serectKey) ?? throw new InvalidDataException();
            redisConfigs.KeyPrefix = MStringExtention.DecryptConfigurationValue(configuration, redisConfigs.KeyPrefix, isSecrectDefault, serectKey) ?? throw new InvalidDataException();

            _ = services.AddSingleton(redisConfigs);
            return services;
        }

        public static IApplicationBuilder UseDefaultMiddleware(this IApplicationBuilder app)
        {
            // Add custom exception handling middleware
            _ = app.UseMiddleware<MAuthenMiddleware>();
            _ = app.UseMiddleware<MExceptionMiddleware>();
            return app;
        }

        public static WebApplicationBuilder AddAutofacConfiguration(this WebApplicationBuilder builder)
        {
            // Configure Autofac as the DI container
            _ = builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            _ = builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                _ = containerBuilder.ResolveDependencyContainer();
            });
            return builder;
        }

        public static IApplicationBuilder ConfigureEndpoints(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                // Enable Swagger in development environment
                _ = app.UseSwagger();
                _ = app.UseSwaggerUI();
            }

            // Map default routes and controllers
            _ = app.MapControllers();
            _ = app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            // Redirect root URL to Swagger UI
            _ = app.MapGet("/", context =>
            {
                context.Response.Redirect("/swagger");
                return Task.CompletedTask;
            });

            return app;
        }

        public static IServiceCollection AddValidateBearerToken<T>(this IServiceCollection services, IConfiguration configuration, string policyUrl = "policy.html")
            where T : MTokenInfo, new()
        {
            return services.ResolveBearerToken<T>(configuration, policyUrl);
        }
    }
}