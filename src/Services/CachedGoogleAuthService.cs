using Microsoft.Extensions.Logging;
using OrderRice.Interfaces;

namespace OrderRice.Services
{
    public class CachedGoogleAuthService : IGoogleAuthService
    {
        private readonly IGoogleAuthService _authService;
        private readonly ILogger<CachedGoogleAuthService> _logger;
        private readonly Constants _constants;
        private const int EXPIRES_IN = 3599;

        public CachedGoogleAuthService(IGoogleAuthService authService, ILogger<CachedGoogleAuthService> logger, Constants constants)
        {
            _authService = authService;
            _constants = constants;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenFromRefreshTokenAsync(CancellationToken cancellationToken)
        {
            if (IsExpriedAccessToken())
            {
                return _constants.AccessToken.Item1;
            }
            _logger.LogInformation("Access token expired.");
            var token = await _authService.GetAccessTokenFromRefreshTokenAsync(cancellationToken);
            _constants.AccessToken = new(token, DateTime.Now.TimeOfDay.TotalSeconds);
            return token;
        }

        private bool IsExpriedAccessToken()
        {
            return _constants.AccessToken.Item2 + EXPIRES_IN > DateTime.Now.TimeOfDay.TotalSeconds;
        }
    }
}
