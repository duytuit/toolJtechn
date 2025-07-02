using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace JtechnApi.Shares
{
    public class RedisService
    {
        private readonly IDatabase _redisDb;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        /// <summary>
        /// Set key-value với optional expire
        /// </summary>
        public async Task SetAsync(string key, string value, TimeSpan? expires = null)
        {
            await _redisDb.StringSetAsync(key, value, expires);
        }

        /// <summary>
        /// Get value theo key
        /// </summary>
        public async Task<string> GetAsync(string key)
        {
            return await _redisDb.StringGetAsync(key);
        }

        /// <summary>
        /// Kiểm tra key có tồn tại không
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
        {
            return await _redisDb.KeyExistsAsync(key);
        }

        /// <summary>
        /// Xoá key
        /// </summary>
        public async Task<bool> RemoveAsync(string key)
        {
            return await _redisDb.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Cập nhật expire cho key
        /// </summary>
        public async Task<bool> ExpireAsync(string key, TimeSpan expires)
        {
            return await _redisDb.KeyExpireAsync(key, expires);
        }
    }
}
