namespace MuonroiBuildingBlock.Startup.AutofacModules
{
    public class AuthContextModule : AutofacModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            _ = builder.RegisterType<AmqpContext>().As<IAmqpContext>().InstancePerLifetimeScope();
            _ = builder.Register(delegate (IComponentContext context)
            {
                ILogger<AuthHeaderHandler> logger = context.Resolve<ILogger<AuthHeaderHandler>>();
                IConfiguration configuration = context.Resolve<IConfiguration>();
                AuthInfoContext authContext = context.Resolve<AuthInfoContext>();
                return new AuthHeaderHandler(logger, authContext, configuration);
            }).InstancePerLifetimeScope();

            _ = builder.Register(delegate (IComponentContext context)
            {
                ILogger<AuthInfoContext> logger = context.Resolve<ILogger<AuthInfoContext>>();
                IHttpContextAccessor httpContextAccessor = context.Resolve<IHttpContextAccessor>();
                AuthInfoContext authContext;
                if (httpContextAccessor?.HttpContext != null)
                {
                    authContext = new AuthInfoContext(httpContextAccessor);
                    logger.LogTrace("Init AuthContext by IHttpContextAccessor: {AuthContext}", authContext);
                }
                else
                {
                    IAmqpContext? amqpContext = context.ResolveOptional<IAmqpContext>();

                    if (amqpContext != null)
                    {
                        authContext = new AuthInfoContext(amqpContext);
                        logger.LogInformation("Init AuthContext by IAmqpContext: {AuthContext}", authContext);
                    }
                    else
                    {
                        authContext = new AuthInfoContext();
                        logger.LogWarning("Failed to initialize AuthContext by IAmqpContext. Using default constructor.");
                    }
                }

                return authContext;
            }).InstancePerLifetimeScope();
        }
    }
}