using System;
using System.Configuration;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace JtecFactory.Helpers
{
    public static class RedisHelper
    {
        private static readonly Lazy<ConnectionMultiplexer> Connection =
            new Lazy<ConnectionMultiplexer>(Connect, true);

        private static ConnectionMultiplexer Multiplexer
        {
            get { return Connection.Value; }
        }

        private static IDatabase Database
        {
            get { return Multiplexer.GetDatabase(); }
        }

        public static async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            ValidateKey(key);
            return await Database.StringSetAsync(key, value, expiry, false, When.Always)
                .ConfigureAwait(false);
        }

        public static async Task<string> GetAsync(string key)
        {
            ValidateKey(key);
            RedisValue value = await Database.StringGetAsync(key).ConfigureAwait(false);
            return value.HasValue ? value.ToString() : null;
        }

        public static async Task<bool> ExistsAsync(string key)
        {
            ValidateKey(key);
            return await Database.KeyExistsAsync(key).ConfigureAwait(false);
        }

        public static async Task<bool> RemoveAsync(string key)
        {
            ValidateKey(key);
            return await Database.KeyDeleteAsync(key).ConfigureAwait(false);
        }

        public static Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan? expiry = null)
        {
            ValidateKey(key);
            return Database.StringSetAsync(key, value, expiry, false, When.NotExists);
        }

        public static void Dispose()
        {
            if (Connection.IsValueCreated)
                Connection.Value.Dispose();
        }

        private static ConnectionMultiplexer Connect()
        {
            string connectionString = ConfigurationManager.AppSettings["RedisConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException(
                    "Không tìm thấy appSettings key 'RedisConnectionString' trong file App.config.");
            }

            ConfigurationOptions options = ConfigurationOptions.Parse(connectionString, true);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Redis key không được để trống.", nameof(key));
        }
    }
}
