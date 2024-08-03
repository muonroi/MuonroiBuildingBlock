namespace MBuildingBlock.Internal
{
    internal static class ApplicationInternalExtention
    {
        internal static IServiceCollection AddControllerConfiguration(this IServiceCollection services, Assembly assemblies)
        {
            _ = services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
            _ = services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new MDateTimeConverter());
            }).AddControllersAsServices();

            //This method must be called after AddControllers
            _ = services.AddFluentValidationAutoValidation();
            _ = services.AddValidatorsFromAssembly(assemblies);

            return services;
        }

        internal static IServiceCollection AddConfigureHttpJson(this IServiceCollection services)
        {
            _ = services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.WriteIndented = false;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            return services;
        }

        internal static IServiceCollection AddAuthContext(this IServiceCollection services)
        {
            _ = services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            return services;
        }

        internal static IServiceCollection AddJwtConfigs(this IServiceCollection services, IConfiguration configuration, MTokenInfo? tokenConfig)
        {
            tokenConfig ??= new MTokenInfo();
            configuration.GetSection(tokenConfig.SectionName).Bind(tokenConfig);
            _ = services.AddSingleton(tokenConfig);
            return services;
        }

        internal static IServiceCollection AddPaginationConfigs(this IServiceCollection services, IConfiguration configuration, MPaginationConfig? paginationConfig)
        {
            paginationConfig ??= new MPaginationConfig();
            configuration.GetSection(paginationConfig.SectionName).Bind(paginationConfig);
            _ = services.AddSingleton(paginationConfig);
            return services;
        }
    }
}