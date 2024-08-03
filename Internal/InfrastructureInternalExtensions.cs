namespace MBuildingBlock.Internal
{
    internal static class InfrastructureInternalExtensions
    {
        internal static IServiceCollection ResolveBearerToken<T>(this IServiceCollection services)
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