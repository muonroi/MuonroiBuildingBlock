

namespace Muonroi.BuildingBlock.Internal.Startup.AutofacModules
{
    internal class AuthContextModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            _ = builder.RegisterType<AmqpContext>().As<IAmqpContext>().InstancePerLifetimeScope();

            _ = builder.Register(delegate (IComponentContext context)
            {
                ILogger<AuthenticateHeaderHandler> logger = context.Resolve<ILogger<AuthenticateHeaderHandler>>();
                IConfiguration configuration = context.Resolve<IConfiguration>();
                MAuthenticateInfoContext authContext = context.Resolve<MAuthenticateInfoContext>();
                return new AuthenticateHeaderHandler(logger, authContext, configuration);
            }).InstancePerLifetimeScope();

            _ = builder.Register(delegate (IComponentContext context)
            {
                ILogger<MAuthenticateInfoContext> logger = context.Resolve<ILogger<MAuthenticateInfoContext>>();
                ResourceSetting resourceSetting = context.Resolve<ResourceSetting>();
                IHttpContextAccessor httpContextAccessor = context.Resolve<IHttpContextAccessor>();
                IConfiguration configuration = context.Resolve<IConfiguration>();
                MAuthenticateInfoContext authContext;
                if (httpContextAccessor?.HttpContext != null)
                {
                    authContext = new MAuthenticateInfoContext(httpContextAccessor, resourceSetting, configuration);
                    logger.LogTrace("Init AuthContext by IHttpContextAccessor: {AuthContext}", authContext);
                }
                else
                {
                    IAmqpContext? amqpContext = context.ResolveOptional<IAmqpContext>();
                    if (amqpContext != null)
                    {
                        authContext = new MAuthenticateInfoContext(amqpContext);
                        logger.LogInformation("Init AuthContext by IAmqpContext: {AuthContext}", authContext);
                    }
                    else
                    {
                        authContext = new MAuthenticateInfoContext(false);
                        logger.LogWarning("Failed to initialize AuthContext by IAmqpContext. Using default constructor.");
                    }
                }

                return authContext;
            }).InstancePerLifetimeScope();
        }
    }
}