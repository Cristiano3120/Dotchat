using DotchatServer.src.Application.Interfaces;
using Org.BouncyCastle.Utilities;
using Serilog;
using StackExchange.Redis;

namespace DotchatServer.src.Infrastructure.Persistence;

public sealed class RedisCache(IConnectionMultiplexer redisConn) : IRedisCache
{
    private readonly IDatabase _database = redisConn.GetDatabase();

    public async Task<bool> SetAsync(string key, string value, TimeSpan expiry)
    {
        try
        {
            return await _database.StringSetAsync(key, value, expiry);
        }
        catch (RedisException ex)
        {
            Log.Error(ex, "Failed to set value in Redis cache for key: {Key}", key);
            return false;
        }
    }

    public async Task<RedisValue> GetAsync<TReturn>(RedisKey key)
    {
        try
        {
            return await _database.StringGetAsync(key);
        }
        catch (RedisException ex)
        {
            Log.Error(ex, "Failed to get value from Redis cache for key: {Key}", key);
            return RedisValue.Null;
        }
    }

    public async Task<bool> DeleteAsync(RedisKey key)
    {
        try
        {
            return await _database.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            Log.Error(ex, "Failed to delete key from Redis cache: {Key}", key);
            return false;
        }
    }
}
