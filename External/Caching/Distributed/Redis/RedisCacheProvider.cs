namespace Muonroi.BuildingBlock.External.Caching.Distributed.Redis
{
    public class RedisCacheProvider(
    IDataSerializer serializer,
    RedisClient client,
    ILogger logger) : ICacheProvider
    {
        private IDataSerializer Serializer { get; } = serializer ?? throw new ArgumentNullException(nameof(serializer));
        private RedisClient Client { get; } = client ?? throw new ArgumentNullException(nameof(client));

        public bool TrySet<TResult>(string key, TResult result, TimeSpan? expired = null)
        {
            try
            {
                Client.Set(key, Serializer.Serialize(
                        new CacheValue<TResult>(result)),
                        expired.HasValue ? (int)expired.Value.TotalSeconds : 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public CacheValue<TResult> TryGet<TResult>(string key)
        {
            try
            {
                string val = Client.Get(key);
                if (string.IsNullOrWhiteSpace(val))
                {
                    TResult? result = default;
                    return new CacheValue<TResult>(result!, false);
                }

                return Serializer.Deserialize<CacheValue<TResult>>(val);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TryGet<TResult> error");
                TResult? result = default;
                return new CacheValue<TResult>(result!, false);
            }
        }

        public void Remove(params string[] keys)
        {
            try
            {
                _ = Client.Del(keys);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Remove error");
            }
        }

        public void RemoveByPrefix(string prefix)
        {
            try
            {
                string[] keysToRemove = Client.Keys(prefix + "*");

                if (keysToRemove?.Length > 0)
                {
                    _ = Client.Del(keysToRemove);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "RemoveByPrefix error");
            }
        }
    }
}