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
                Type? specificInterface = businessService.GetInterfaces()
                    .FirstOrDefault(i => !i.IsGenericType &&
                                         i.GetInterfaces().Any(ii => ii.IsGenericType &&
                                                                     (ii.GetGenericTypeDefinition() == typeof(IMRepository<>) ||
                                                                      ii.GetGenericTypeDefinition() == typeof(IMQueries<>))));

                if (specificInterface != null)
                {
                    _ = services.AddScoped(specificInterface, businessService);
                }
                else
                {
                    Type? genericInterface = businessService.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType &&
                                             (i.GetGenericTypeDefinition() == typeof(IMRepository<>) ||
                                              i.GetGenericTypeDefinition() == typeof(IMQueries<>)));

                    if (genericInterface != null)
                    {
                        _ = services.AddScoped(genericInterface, businessService);
                    }
                }
            }

            return services;
        }
    }
}