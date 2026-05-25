using StackExchange.Redis;

namespace DotchatServer.src.Application.Interfaces;

public interface IRedisCache
{
    Task<bool> ExistsAsync(RedisKey key);
    Task<bool> SetAsync(RedisKey key, RedisValue value, TimeSpan expiry);

    Task<RedisValue> GetAsync(RedisKey key);

    Task<bool> DeleteAsync(RedisKey key);
}