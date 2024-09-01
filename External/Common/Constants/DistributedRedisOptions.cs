namespace Muonroi.BuildingBlock.External.Common.Constants
{
    public static class DistributedRedisOptions
    {
        public static readonly DistributedCacheEntryOptions DefaultCacheOptions_10_5 = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };
    }
}