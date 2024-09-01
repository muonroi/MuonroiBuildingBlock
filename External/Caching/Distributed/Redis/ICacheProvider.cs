namespace Muonroi.BuildingBlock.External.Caching.Distributed.Redis
{
    public interface ICacheProvider : Dapper.Extensions.Caching.ICacheProvider
    {
        void RemoveByPrefix(string prefix);
    }
}