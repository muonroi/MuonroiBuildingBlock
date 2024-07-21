namespace MuonroiBuildingBlock.Contract.Interfaces
{
    public interface IJsonSerializeService
    {
        string Serialize<T>(T obj);

        string Serialize<T>(T obj, Type type);

        T? Deserialize<T>(string text);
    }
}