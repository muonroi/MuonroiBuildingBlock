namespace Muonroi.BuildingBlock.Internal
{
    internal static class InfrastructureInternalExtensions
    {
        internal static IServiceCollection ResolveBearerToken<T>(this IServiceCollection services, string policyUrl = "policy.html")
            where T : MTokenInfo
        {
            // Build the service provider to resolve dependencies
            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            T jwtConfig = serviceProvider.GetRequiredService<T>();

            if (string.IsNullOrEmpty(jwtConfig.SigningKeys))
            {
                throw new Exception("SecretKey is null or empty");
            }

            // Configure API versioning with sunset policy
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

            // Configure RSA for JWT token validation
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(jwtConfig.PublicKey.ToCharArray());

            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ClockSkew = TimeSpan.Zero
            };

            // Configure JWT Bearer authentication
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