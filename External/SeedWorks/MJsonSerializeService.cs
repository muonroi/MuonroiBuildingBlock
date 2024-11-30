
namespace Muonroi.BuildingBlock.External.SeedWorks
{
    internal class MJsonSerializeService : IMJsonSerializeService
    {
        public string Serialize<T>(T obj)
        {
            return NewtonsoftJsonSerializer.SerializeObject(obj);
        }

        public T? Deserialize<T>(string text)
        {
            return NewtonsoftJsonSerializer.DeserializeObject<T>(text);
        }
    }
}