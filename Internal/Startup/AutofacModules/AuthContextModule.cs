namespace MBuildingBlock.Internal.Startup.AutofacModules
{
    internal class AuthContextModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            _ = builder.RegisterType<AmqpContext>().As<IAmqpContext>().InstancePerLifetimeScope();

            _ = builder.Register(delegate (IComponentContext context)
            {
                ILogger<AuthHeaderHandler> logger = context.Resolve<ILogger<AuthHeaderHandler>>();
                IConfiguration configuration = context.Resolve<IConfiguration>();
                MAuthInfoContext authContext = context.Resolve<MAuthInfoContext>();
                return new AuthHeaderHandler(logger, authContext, configuration);
            }).InstancePerLifetimeScope();

            _ = builder.Register(delegate (IComponentContext context)
            {
                ILogger<MAuthInfoContext> logger = context.Resolve<ILogger<MAuthInfoContext>>();
                ResourceSetting resourceSetting = context.Resolve<ResourceSetting>();
                IHttpContextAccessor httpContextAccessor = context.Resolve<IHttpContextAccessor>();
                MAuthInfoContext authContext;
                if (httpContextAccessor?.HttpContext != null)
                {
                    authContext = new MAuthInfoContext(httpContextAccessor, resourceSetting);
                    logger.LogTrace("Init AuthContext by IHttpContextAccessor: {AuthContext}", authContext);
                }
                else
                {
                    IAmqpContext? amqpContext = context.ResolveOptional<IAmqpContext>();

                    if (amqpContext != null)
                    {
                        authContext = new MAuthInfoContext(amqpContext);
                        logger.LogInformation("Init AuthContext by IAmqpContext: {AuthContext}", authContext);
                    }
                    else
                    {
                        authContext = new MAuthInfoContext();
                        logger.LogWarning("Failed to initialize AuthContext by IAmqpContext. Using default constructor.");
                    }
                }

                return authContext;
            }).InstancePerLifetimeScope();
        }
    }
}