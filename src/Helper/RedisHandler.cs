using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace OrderRice.Helper
{
    public class RedisHandler
    {
        RedisConnectorHelper _connectionHelper;
        public RedisHandler(IConfiguration _configuration)
        {
            _connectionHelper = new RedisConnectorHelper(_configuration);
        }

        public async Task<string> ReadAccessToken()
        {
            RedisKey key = new("accessToken");
            var cache = _connectionHelper.Connection.GetDatabase();
            return await cache.StringGetAsync(key);
        }

        public async Task<bool> WriteAccessToken(string accessToken, int expiredTime)
        {
            RedisKey key = new("accessToken");
            var cache = _connectionHelper.Connection.GetDatabase();
            var expirationTime = TimeSpan.FromSeconds(expiredTime - 100);
            return await cache.StringSetAsync(key, accessToken, expirationTime);
        }
    }
}
