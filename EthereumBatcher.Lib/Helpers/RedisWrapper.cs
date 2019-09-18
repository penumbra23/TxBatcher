using StackExchange.Redis;

namespace EthereumBatcher.Lib.Helpers
{
    /// <summary>
    /// Simple wrapper for Redis cache.
    /// </summary>
    public class RedisWrapper
    {
        public RedisWrapper(string redisConnectionString)
        {
            if (RedisClient == null)
                RedisClient = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        protected static ConnectionMultiplexer RedisClient { get; private set; }

        public string Get(string key) => RedisClient.GetDatabase().StringGet(key);

        public bool Set(string key, string value) => RedisClient.GetDatabase().StringSet(key, value);

        public ITransaction CreateTransaction() => RedisClient.GetDatabase().CreateTransaction();
    }
}
