namespace MBuildingBlock.External.Interfaces
{
    public interface IMJsonSerializeService
    {
        string Serialize<T>(T obj);

        string Serialize<T>(T obj, Type type);

        T? Deserialize<T>(string text);
    }
}