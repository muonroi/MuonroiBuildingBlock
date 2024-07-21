namespace MuonroiBuildingBlock.Infrastructure
{
    public static class InfrastructureExtensions
    {
        public static readonly Assembly? entryAsembly = Assembly.GetEntryAssembly();

        private static AutofacServiceProvider? _autofac;

        public static IServiceCollection AddInfrastructure(this IServiceCollection services
            , IConfiguration configuration)
        {
            _ = services.AddCultureProviders();
            _ = services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                }).AddControllersAsServices();
            _ = services.AddEndpointsApiExplorer();
            _ = services.AddSwaggerGen();
            _ = services.AddHttpContextAccessor();
            _ = services.AddProblemDetails();
            _ = services.AddConfigureHttpJson();
            _ = services.AddSingleton<IJsonSerializeService, JsonSerializeService>();
            _ = services.AddSingleton<IDateTimeService, DateTimeService>();
            _ = services.AddDapperConnectionStringProvider<ConnectionStringProvider>();
            _ = services.AddPaginationConfigs(configuration);
            _ = services.AddJwtConfig(configuration);
            _ = services.AddSystemConfig(configuration);
            _ = services.AddAuthContext();
            SqlMapper.AddTypeHandler(new TrimStringHandler());
            return services;
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

        public static IServiceCollection AddJwtConfig(this IServiceCollection services, IConfiguration configuration)
        {
            JwtConfig jwtConfig = new();
            configuration.GetSection(JwtConfig.SectionName).Bind(jwtConfig);
            _ = services.AddSingleton(jwtConfig);
            return services;
        }

        public static IServiceCollection AddPaginationConfigs(this IServiceCollection services, IConfiguration configuration)
        {
            PaginationConfigs paginationConfigs = new();
            configuration.GetSection(PaginationConfigs.SectionName).Bind(paginationConfigs);
            _ = services.AddSingleton(paginationConfigs);
            return services;
        }

        public static IServiceCollection AddConfigureHttpJson(this IServiceCollection services)
        {
            _ = services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.WriteIndented = false;
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            return services;
        }

        public static IServiceProvider AddAutofacConfigure(this IServiceCollection services, IConfiguration configuration)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            JwtConfig jwtConfig = serviceProvider.GetRequiredService<JwtConfig>();
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Console.WriteLine("ExecutingAssembly: " + executingAssembly.FullName);
            Console.WriteLine("EntryAsembly: " + entryAsembly?.FullName);
            AddAuthContext(services);

            _ = services.AddApiVersioning(delegate (ApiVersioningOptions opt)
            {
                opt.ErrorResponses = new APIVersionErrorResponseProvider();
            });
            IdentityModelEventSource.ShowPII = true;
            RSA rsa = RSA.Create();
            if (string.IsNullOrEmpty(jwtConfig.SigningKeys))
            {
                throw new Exception("SecretKey is null or empty");
            }
            rsa.ImportFromPem(configuration.GetCryptConfigValue(ConfigurationSetting.PublicKey)!);
            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ClockSkew = TimeSpan.Zero
            };
            _ = services.AddAuthentication(delegate (AuthenticationOptions x)
            {
                x.DefaultAuthenticateScheme = "Bearer";
                x.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(delegate (JwtBearerOptions x)
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = validationParameters;
            });
            ContainerBuilder containerBuilder = new();
            containerBuilder.Populate(services);
            _ = containerBuilder.RegisterModule(new MediatorModule());
            _ = containerBuilder.RegisterModule(new AuthContextModule());
            ConfigureContainer(containerBuilder);
            ConfigureRefitServices(containerBuilder);
            Autofac.IContainer lifetimeScope = containerBuilder.Build();
            _autofac = new AutofacServiceProvider(lifetimeScope);
            return _autofac;
        }

        public static void ConfigureRefitServices(ContainerBuilder builder)
        {
        }

        public static void ConfigureContainer(ContainerBuilder builder)
        {
        }

        private static void AddAuthContext(IServiceCollection services)
        {
            _ = services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}