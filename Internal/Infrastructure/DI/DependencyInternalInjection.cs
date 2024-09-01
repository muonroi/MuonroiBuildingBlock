namespace Muonroi.BuildingBlock.Internal.Infrastructure.DI
{
    internal static class DependencyInternalInjection
    {
        internal static IServiceCollection ResolveDependencyScope(this IServiceCollection services, Assembly assembly)
        {
            // Get all the types from the assembly that are classes, non-abstract, and not generic.
            IEnumerable<Type> businessServices = assembly.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType)
                .Where(x => x.GetInterfaces().Any(i => i.IsGenericType &&
                                                        (i.GetGenericTypeDefinition() == typeof(IMRepository<>) ||
                                                         i.GetGenericTypeDefinition() == typeof(IMQueries<>))));

            foreach (Type? businessService in businessServices)
            {
                // Find the specific interface that extends IMRepository<> or IMQueries<>.
                Type? specificInterface = businessService.GetInterfaces()
                    .FirstOrDefault(i => !i.IsGenericType && // Ensure it's not a generic interface like IMRepository<>
                                         i.GetInterfaces().Any(ii => ii.IsGenericType &&
                                                                     (ii.GetGenericTypeDefinition() == typeof(IMRepository<>) ||
                                                                      ii.GetGenericTypeDefinition() == typeof(IMQueries<>))));

                // Register the service with the specific interface if found
                if (specificInterface != null)
                {
                    _ = services.AddScoped(specificInterface, businessService);
                }
                else
                {
                    // Fallback: Register the generic interface directly if no specific interface is found
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