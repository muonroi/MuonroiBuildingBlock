namespace MuonroiBuildingBlock.Startup
{
    public class BaseStartup(IConfiguration configuration)
    {
        public Assembly? entryAsembly = Assembly.GetEntryAssembly();

        private AutofacServiceProvider? _autofac;

        public IConfiguration Configuration { get; } = configuration;

        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Console.WriteLine("ExecutingAssembly: " + executingAssembly.FullName);
            Console.WriteLine("EntryAsembly: " + entryAsembly?.FullName);
            AddAuthContext(services);

            _ = services.AddApiVersioning(delegate (ApiVersioningOptions opt)
            {
                opt.ErrorResponses = new APIVersionErrorResponseProvider();
            });
            IdentityModelEventSource.ShowPII = true;
            RSA rSA = RSA.Create();
            string envPublicKey = Environment.GetEnvironmentVariable("ENV_PUBLIC_KEY") ?? string.Empty;
            if (string.IsNullOrEmpty(envPublicKey))
            {
                throw new Exception("Public key is null or empty");
            }
            rSA.ImportRSAPublicKey(Convert.FromBase64String(envPublicKey), out _);
            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = new RsaSecurityKey(rSA),
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

        protected virtual void ConfigureRefitServices(ContainerBuilder builder)
        {
        }

        public virtual void ConfigureContainer(ContainerBuilder builder)
        {
        }

        private static void AddAuthContext(IServiceCollection services)
        {
            _ = services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}