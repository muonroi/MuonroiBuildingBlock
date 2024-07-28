namespace MBuildingBlock.External.SeedWorks
{
    internal class MJsonSerializeService : IMJsonSerializeService
    {
        private static readonly JsonSerializerOptions _defaultOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public string Serialize<T>(T obj)
        {
            return TextJson.Serialize(obj, _defaultOptions);
        }

        public string Serialize<T>(T obj, Type type)
        {
            return TextJson.Serialize(obj, type, _defaultOptions);
        }

        public T? Deserialize<T>(string text)
        {
            return TextJson.Deserialize<T>(text);
        }
    }
}