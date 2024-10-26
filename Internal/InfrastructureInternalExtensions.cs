namespace Muonroi.BuildingBlock.Internal
{
    internal static class InfrastructureInternalExtensions
    {
        internal static IServiceCollection ResolveBearerToken<T, TPermission>(this IServiceCollection services, IConfiguration configuration, string policyUrl = "policy.html")
       where T : MTokenInfo, new()
        where TPermission : Enum
        {
            T nameSection = new();
            _ = services.Configure<T>(configuration.GetSection(nameSection.SectionName));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            T jwtConfig = serviceProvider.GetRequiredService<IOptions<T>>().Value;

            jwtConfig.Issuer = configuration.GetCryptConfigValueCipherText(jwtConfig.Issuer)!;
            jwtConfig.Audience = configuration.GetCryptConfigValueCipherText(jwtConfig.Audience)!;
            jwtConfig.SigningKeys = configuration.GetCryptConfigValueCipherText(jwtConfig.SigningKeys)!;

            if (string.IsNullOrEmpty(jwtConfig.SigningKeys))
            {
                throw new Exception("SigningKeys is null or empty");
            }

            _ = services.AddSingleton<MAuthenticateTokenHelper<TPermission>>();

            _ = services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                _ = options.Policies.Sunset(0.9)
                    .Effective(DateTimeOffset.Now.AddDays(60))
                    .Link(policyUrl)
                    .Title("Versioning Policy")
                    .Type("text/html");
            });

            IdentityModelEventSource.ShowPII = true;

            RSA rsa = RSA.Create();
            rsa.ImportFromPem(jwtConfig.PublicKey.ToCharArray());

            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience
            };

            _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = validationParameters;
            });

            _ = services.AddAuthorization();

            return services;
        }

        internal static ContainerBuilder ResolveDependencyContainer(this ContainerBuilder builder)
        {
            // Register application modules to Autofac container
            _ = builder.RegisterModule(new MediatorModule());
            _ = builder.RegisterModule(new AuthContextModule());
            return builder;
        }

    }
}