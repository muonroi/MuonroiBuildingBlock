namespace Muonroi.BuildingBlock.External.SeedWorks
{
    internal class MJsonSerializeService : IMJsonSerializeService
    {
        public string Serialize<T>(T obj)
        {
            return JsonHandler.Serialize(obj);
        }

        public string Serialize<T>(T obj, Type type)
        {
            return JsonHandler.Serialize(type, obj);
        }

        public T? Deserialize<T>(string text)
        {
            return JsonHandler.Deserialize<T>(text);
        }
    }
}