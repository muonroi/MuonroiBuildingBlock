namespace MBuildingBlock.External
{
    public static class InfrastructureExtensions
    {
        public static readonly Assembly? entryAsembly = Assembly.GetEntryAssembly();

        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration,
            MTokenInfo? tokenConfig = null,
            MPaginationConfig? paginationConfigs = null)
        {
            _ = services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                }).AddControllersAsServices();
            _ = services.AddEndpointsApiExplorer();
            _ = services.AddSwaggerGen();
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

        public static WebApplicationBuilder AddAutofacConfiguration(this WebApplicationBuilder builder)
        {
            _ = builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            _ = builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            {
                _ = builder.RegisterModule(new MediatorModule());
                _ = builder.RegisterModule(new AuthContextModule());
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

        public static IServiceCollection AddValidateBearerToken<T>(this IServiceCollection services)
            where T : MTokenInfo
        {
            return services.ResolveBearerToken<T>();
        }
    }
}