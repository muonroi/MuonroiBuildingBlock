namespace Muonroi.BuildingBlock.External.DI;
public class ConfigurationAutoMapperProfiles : Profile
{
    private readonly string _solutionNamespacePrefix;

    public ConfigurationAutoMapperProfiles(Assembly assembly, string solutionNamespacePrefix)
    {
        _solutionNamespacePrefix = solutionNamespacePrefix;
        ApplyMappingsFromAssembly(assembly);
    }

    public ConfigurationAutoMapperProfiles(string solutionNamespacePrefix)
    {
        _solutionNamespacePrefix = solutionNamespacePrefix;
        ApplyMappingsFromAllAssemblies();
    }

    private void ApplyMappingsFromAllAssemblies()
    {
        List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly?.FullName != null && assembly.FullName.StartsWith(_solutionNamespacePrefix))
            .ToList();

        foreach (Assembly? assembly in assemblies)
        {
            ApplyMappingsFromAssembly(assembly);
        }
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        List<Type> types = assembly.GetExportedTypes()
                            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
                            .ToList();

        foreach (Type? type in types)
        {
            IEnumerable<Type> mapFromInterfaces = type.GetInterfaces()
                                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>));

            foreach (Type? mapFromInterface in mapFromInterfaces)
            {
                Type sourceType = mapFromInterface.GetGenericArguments()[0];
                _ = CreateMap(sourceType, type);
                _ = CreateMap(type, sourceType);
            }
        }
    }
}
