using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JtechnApi.Shares
{
    public class RedisService
    {
        private readonly IDatabase _redisDb;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public RedisService(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        public async Task SetAsync(string key, string value, TimeSpan? expires = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _redisDb.StringSetAsync(key, value, expires);
        }

        public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _redisDb.StringGetAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expires = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _redisDb.StringSetAsync(key, json, expires);
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = await _redisDb.StringGetAsync(key);
            return json.HasValue ? JsonSerializer.Deserialize<T>(json, _jsonOptions) : default;
        }

        public async Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan? expires = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _redisDb.StringSetAsync(key, value, expires, When.NotExists);
        }

        public async Task<(bool Found, string Value)> TryGetAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await _redisDb.StringGetAsync(key);
            return (value.HasValue, value);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _redisDb.KeyExistsAsync(key);
        }

        public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _redisDb.KeyDeleteAsync(key);
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expires, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _redisDb.KeyExpireAsync(key, expires);
        }
    }
}
