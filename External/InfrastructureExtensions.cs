namespace MBuildingBlock.External
{
    public static class InfrastructureExtensions
    {
        public static readonly Assembly? entryAsembly = Assembly.GetEntryAssembly();

        public static IServiceCollection AddInfrastructure<TToken, TPaging>(this IServiceCollection services,
            IConfiguration configuration,
            TToken tokenConfig,
            TPaging paginationConfigs)
            where TToken : MTokenInfo
            where TPaging : MPaginationConfigs
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
            _ = services.AddJwtConfig(configuration, tokenConfig);
            _ = services.AddSystemConfig(configuration);
            _ = services.AddAuthContext();
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

        public static IServiceCollection AddJwtConfig<T>(this IServiceCollection services, IConfiguration configuration, T jwtValue)
            where T : MTokenInfo
        {
            configuration.GetSection(jwtValue.SectionName).Bind(jwtValue);
            _ = services.AddSingleton(jwtValue);
            return services;
        }

        public static IServiceCollection AddPaginationConfigs<T>(this IServiceCollection services, IConfiguration configuration, T pageinationConfig)
            where T : MPaginationConfigs
        {
            configuration.GetSection(pageinationConfig.SectionName).Bind(pageinationConfig);
            _ = services.AddSingleton(pageinationConfig);
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

        public static IServiceCollection AddValidateBearerToken<T>(this IServiceCollection services)
            where T : MTokenInfo
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            T jwtConfig = serviceProvider.GetRequiredService<T>();
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
            rsa.ImportFromPem(jwtConfig.PublicKey.ToCharArray());
            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true, // Đặt là true để validate khóa
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
            _ = services.AddAuthorization();

            return services;
        }
    }
}