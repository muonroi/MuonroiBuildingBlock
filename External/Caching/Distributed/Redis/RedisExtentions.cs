

namespace Muonroi.BuildingBlock.External.Caching.Distributed.Redis;

public static class RedisExtentions
{
    public static IServiceCollection AddDapperCaching(this IServiceCollection services, IConfiguration configuration,
        RedisConfigs redisConfigs)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (!redisConfigs.Enable)
        {
            return services;
        }

        // Initialize RedisClient with configurations
        RedisClient redisClient = new(new ConnectionStringBuilder
        {
            Host = $"{redisConfigs.Host}:{redisConfigs.Port}",
            Password = redisConfigs.Password,
        });

        // Add Dapper Caching in Redis with the provided configurations
        _ = services.AddDapperCachingInRedis(new RedisConfiguration
        {
            AllMethodsEnableCache = redisConfigs.AllMethodsEnableCache,
            Expire = TimeSpan.FromMinutes(redisConfigs.Expire),
            KeyPrefix = redisConfigs.KeyPrefix,
        }, redisClient);

        // Register the RedisCacheProvider
        _ = services.AddSingleton<Dapper.Extensions.Caching.ICacheProvider, RedisCacheProvider>();

        return services;
    }

    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration,
        RedisConfigs redisConfigs)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrEmpty(redisConfigs.Password) || string.IsNullOrEmpty(redisConfigs.Host) || string.IsNullOrEmpty(redisConfigs.Port))
        {
            throw new InvalidConfigurationException($"Invalid {RedisConfigs.DefaultSectionName}");
        }

        // Add StackExchange Redis Cache with the configurations
        _ = services.AddStackExchangeRedisCache(option =>
        {
            option.InstanceName = redisConfigs.KeyPrefix ?? string.Empty;
            option.ConfigurationOptions = new ConfigurationOptions
            {
                EndPoints = { { redisConfigs.Host, int.Parse(redisConfigs.Port) } },
                Password = redisConfigs.Password,
                AllowAdmin = redisConfigs.AllowAdmin,
                AbortOnConnectFail = redisConfigs.AbortOnConnectFail,
            };
        });

        // Register RedisClient with the configurations
        _ = services.AddSingleton(sp =>
        {
            return new RedisClient($"{redisConfigs.Host}:{redisConfigs.Port},password={redisConfigs.Password}");
        });

        return services;
    }

    public static async Task<string?> GetCacheAsync(this IDistributedCache distributedCache, string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(distributedCache);
        ArgumentException.ThrowIfNullOrEmpty(key);

        byte[]? cacheValue = await distributedCache.GetAsync(key, token);
        return cacheValue is not null ? Encoding.UTF8.GetString(cacheValue) : default;
    }

    public static async Task<T?> GetCacheAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(distributedCache);
        ArgumentException.ThrowIfNullOrEmpty(key);

        byte[]? cacheValue = await distributedCache.GetAsync(key, token);
        if (cacheValue is not null)
        {
            string valueString = Encoding.UTF8.GetString(cacheValue);
            return JsonSerializer.Deserialize<T>(valueString);
        }
        return default;
    }

    public static async Task SetCacheAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions? options,
        CancellationToken cancelationToken = default)
    {
        ArgumentNullException.ThrowIfNull(distributedCache);
        ArgumentException.ThrowIfNullOrEmpty(key);

        string serializeValue = JsonSerializer.Serialize(value);
        byte[] saveValue = Encoding.UTF8.GetBytes(serializeValue);

        await distributedCache.SetAsync(key, saveValue, options ?? new(), cancelationToken);
    }

    public static async Task RemoveAsync(this IDistributedCache distributedCache, string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(distributedCache);
        ArgumentException.ThrowIfNullOrEmpty(key);

        await distributedCache.RemoveAsync(key, token);
    }

    public static async Task RefreshAsync(this IDistributedCache distributedCache, string key, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(distributedCache);
        ArgumentException.ThrowIfNullOrEmpty(key);

        await distributedCache.RefreshAsync(key, token);
    }

    public static async Task<T?> GetOrSetAsync<T>(this IDistributedCache distributedCache
        , string key
        , Func<Task<T>> cacheData
        , CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(distributedCache);

            byte[]? cacheValue = await distributedCache.GetAsync(key, cancellationToken);
            if (cacheValue != null)
            {
                string valueString = Encoding.UTF8.GetString(cacheValue);
                return string.IsNullOrEmpty(valueString) ? default : JsonSerializer.Deserialize<T>(valueString);
            }
            else
            {
                T? data = await cacheData();
                string serializeValue = JsonSerializer.Serialize(data);
                byte[] saveValue = Encoding.UTF8.GetBytes(serializeValue);

                await distributedCache.SetAsync(key, saveValue, token: cancellationToken);
                return data;
            }
        }
        catch
        {
            return default;
        }
    }
}