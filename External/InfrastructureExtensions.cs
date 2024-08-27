namespace Muonroi.BuildingBlock.External
{
    public static class InfrastructureExtensions
    {
        public static readonly Assembly? entryAssembly = Assembly.GetEntryAssembly();

        public static IServiceCollection AddInfrastructure<TProgram>(this IServiceCollection services,
            IConfiguration configuration,
            MTokenInfo? tokenConfig = null,
            MPaginationConfig? paginationConfigs = null)
        {
            _ = services.AddControllers(options =>
            {
                _ = options.Filters.Add<DefaultProducesResponseTypeFilter>();
            })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                }).AddControllersAsServices();
            _ = services.AddEndpointsApiExplorer();
            _ = services.AddSwaggerGen(options =>
            {
                options.OperationFilter<SwaggerDefaultValues>();
                string fileName = typeof(TProgram).Assembly.GetName().Name + ".xml";
                string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                options.IncludeXmlComments(filePath);
            });
            _ = services.AddHttpContextAccessor();
            _ = services.AddProblemDetails();
            _ = services.AddSingleton<IMJsonSerializeService, MJsonSerializeService>();
            _ = services.AddSingleton<IMDateTimeService, MDateTimeService>();
            _ = services.AddPaginationConfigs(configuration, paginationConfigs);
            _ = services.AddJwtConfigs(configuration, tokenConfig);
            _ = services.AddSystemConfig(configuration);
            _ = services.AddAuthContext();
            return services;
        }

        public static IApplicationBuilder UseDefaultMiddleware(this IApplicationBuilder app)
        {
            _ = app.UseMiddleware<MExceptionMiddleware>();
            return app;
        }

        public static WebApplicationBuilder AddAutofacConfiguration(this WebApplicationBuilder builder)
        {
            _ = builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            _ = builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                _ = builder.ResolveDependencyContainer();
            });
            return builder;
        }

        public static IApplicationBuilder ConfigureEndpoints(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                _ = app.UseSwagger();
                _ = app.UseSwaggerUI();
            }
            _ = app.MapControllers();

            _ = app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            _ = app.MapGet("/", context =>
            {
                context.Response.Redirect("/swagger");
                return Task.CompletedTask;
            });

            return app;
        }

        public static IServiceCollection AddDatabaseConfig(this IServiceCollection services, IConfiguration configuration, string nameConfig = nameof(DatabaseConfigs))
        {
            DatabaseConfigs? databaseConfigs = configuration.GetSection(nameConfig).Get<DatabaseConfigs>() ?? throw new InvalidOperationException("Database configuration is missing or cannot be bound.");
            _ = services.AddSingleton(databaseConfigs);

            return services;
        }

        public static IServiceCollection AddValidateBearerToken<T>(this IServiceCollection services, string policyUrl = "polycy.html")
            where T : MTokenInfo
        {
            return services.ResolveBearerToken<T>(policyUrl: policyUrl);
        }
    }
}