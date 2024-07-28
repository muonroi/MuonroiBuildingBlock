namespace MBuildingBlock.External.DI
{
    public static class ConfigurationDepedencyInjection
    {
        public static IServiceCollection AddScopeServices(this IServiceCollection services, Assembly assembly)
        {
            IEnumerable<Type> businessServices = assembly.GetTypes()
            .Where(x => x.GetInterfaces().Any(i => i.Name == typeof(IMRepository<>).Name || i.Name == typeof(IMQueries<>).Name)
            && !x.IsAbstract && x.IsClass && !x.IsGenericType);

            foreach (Type? businessService in businessServices)
            {
                Type[] allInterfaces = businessService.GetInterfaces();
                Type? directInterface = allInterfaces.Except(allInterfaces.SelectMany(t => t.GetInterfaces())).FirstOrDefault();
                if (directInterface != null)
                {
                    services.Add(new ServiceDescriptor(directInterface, businessService, ServiceLifetime.Scoped));
                }
            }
            return services;
        }
    }
}