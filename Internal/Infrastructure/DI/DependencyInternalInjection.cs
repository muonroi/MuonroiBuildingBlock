namespace Muonroi.BuildingBlock.Internal.Infrastructure.DI
{
    internal static class DependencyInternalInjection
    {
        internal static IServiceCollection ResolveDependencyScope(this IServiceCollection services, Assembly assembly)
        {
            IEnumerable<Type> businessServices = assembly.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType)
                .Where(x => x.GetInterfaces().Any(i => i.IsGenericType &&
                                                        (i.GetGenericTypeDefinition() == typeof(IMRepository<>) ||
                                                         i.GetGenericTypeDefinition() == typeof(IMQueries<>))));

            foreach (Type? businessService in businessServices)
            {
                Type? directInterface = businessService.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType &&
                                         (i.GetGenericTypeDefinition() == typeof(IMRepository<>) ||
                                          i.GetGenericTypeDefinition() == typeof(IMQueries<>)));

                if (directInterface != null)
                {
                    _ = services.AddScoped(directInterface, businessService);
                }
            }

            return services;
        }
    }
}