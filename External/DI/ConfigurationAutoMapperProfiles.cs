namespace Muonroi.BuildingBlock.External.DI
{
    internal class ConfigurationAutoMapperProfiles : Profile
    {
        public ConfigurationAutoMapperProfiles(Assembly assembly)
        {
            ApplyMappingsFromAssembly(assembly);
        }
        public void ApplyMappingsFromAssembly(Assembly assembly)
        {
            List<Type> types = assembly.GetExportedTypes()
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
                .ToList();

            foreach (Type? type in types)
            {
                IEnumerable<Type> interfaces = type.GetInterfaces().Where(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>));

                foreach (Type? mapInterface in interfaces)
                {
                    Type sourceType = mapInterface.GetGenericArguments()[0];

                    _ = CreateMap(sourceType, type);
                    _ = CreateMap(type, sourceType);
                }
            }
        }
    }
}
