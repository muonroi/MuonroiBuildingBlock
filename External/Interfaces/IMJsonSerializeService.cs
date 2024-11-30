namespace Muonroi.BuildingBlock.External.Interfaces
{
    public interface IMJsonSerializeService
    {
        string Serialize<T>(T obj);

        T? Deserialize<T>(string text);
    }
}