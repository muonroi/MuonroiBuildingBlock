namespace Muonroi.BuildingBlock.External
{
    public static class InfrastructureExtensions
    {
        public static readonly Assembly? EntryAssembly = Assembly.GetEntryAssembly();

        public static IServiceCollection AddInfrastructure<TProgram>(this IServiceCollection services,
            IConfiguration configuration,
            MTokenInfo? tokenConfig = null,
            MPaginationConfig? paginationConfigs = null)
        {
            // Add essential services to the DI container
            _ = services.AddControllers(options =>
            {
                _ = options.Filters.Add<DefaultProducesResponseTypeFilter>();
            })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                })
                .AddControllersAsServices();

            // Register various services for API endpoints and utilities
            _ = services.AddEndpointsApiExplorer()
                    .AddSwaggerGen(options =>
                    {
                        options.OperationFilter<SwaggerDefaultValues>();
                        string fileName = typeof(TProgram).Assembly.GetName().Name + ".xml";
                        string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                        options.IncludeXmlComments(filePath);
                    })
                    .AddHttpContextAccessor()
                    .AddProblemDetails()
                    .AddSingleton<IMJsonSerializeService, MJsonSerializeService>()
                    .AddSingleton<IMDateTimeService, MDateTimeService>()
                    .AddPaginationConfigs(configuration, paginationConfigs)
                    .AddJwtConfigs(configuration, tokenConfig)
                    .AddSystemConfig(configuration)
                    .AddAuthContext();

            return services;
        }

        public static IApplicationBuilder UseDefaultMiddleware(this IApplicationBuilder app)
        {
            // Add custom exception handling middleware
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

        public static IServiceCollection AddValidateBearerToken<T>(this IServiceCollection services, string policyUrl = "policy.html")
            where T : MTokenInfo
        {
            return services.ResolveBearerToken<T>(policyUrl);
        }
    }
}