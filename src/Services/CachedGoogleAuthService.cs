using Microsoft.Extensions.Logging;
using OrderLunch.Helper;
using OrderLunch.Interfaces;

namespace OrderLunch.Services
{
    public class CachedGoogleAuthService : IGoogleAuthService
    {
        private readonly IGoogleAuthService _authService;
        private readonly ILogger<CachedGoogleAuthService> _logger;
        private readonly RedisHandler _redisHandler;
        private readonly Constants _constants;

        public CachedGoogleAuthService(IGoogleAuthService authService, ILogger<CachedGoogleAuthService> logger, RedisHandler redisHandler, Constants constants)
        {
            _authService = authService;
            _logger = logger;
            _redisHandler = redisHandler;
            _constants = constants;
        }

        public async Task<string> GetAccessTokenFromRefreshTokenAsync(CancellationToken cancellationToken)
        {
            string accessToken = await _redisHandler.ReadAccessToken();
            if (!string.IsNullOrEmpty(accessToken))
            {
                return accessToken;
            }
            _logger.LogInformation("Renew access token");
            var token = await _authService.GetAccessTokenFromRefreshTokenAsync(cancellationToken);
            await _redisHandler.WriteAccessToken(token, _constants.EXPIRES_IN);
            return token;
        }
    }
}
