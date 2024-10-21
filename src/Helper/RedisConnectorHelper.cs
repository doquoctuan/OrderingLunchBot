using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace OrderLunch.Helper
{
    public class RedisConnectorHelper
    {
        public RedisConnectorHelper(IConfiguration _configuration)
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(_configuration["RedisUrl"]);
            });
        }

        private Lazy<ConnectionMultiplexer> lazyConnection;

        public ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}
