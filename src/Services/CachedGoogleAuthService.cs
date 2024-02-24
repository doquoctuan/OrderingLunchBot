using Microsoft.Extensions.Logging;
using OrderRice.Interfaces;

namespace OrderRice.Services
{
    public class CachedGoogleAuthService : IGoogleAuthService
    {
        private readonly IGoogleAuthService _authService;
        private readonly ILogger<CachedGoogleAuthService> _logger;
        private readonly Constants _constants;

        public CachedGoogleAuthService(IGoogleAuthService authService, ILogger<CachedGoogleAuthService> logger, Constants constants)
        {
            _authService = authService;
            _constants = constants;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenFromRefreshTokenAsync(CancellationToken cancellationToken)
        {
            (string accessToken, double createDate) = _constants.AccessToken;
            if (!IsExpriedAccessToken(createDate))
            {
                return accessToken;
            }
            _logger.LogInformation("Renew access token");
            var token = await _authService.GetAccessTokenFromRefreshTokenAsync(cancellationToken);
            _constants.AccessToken = new(token, DateTime.Now.TimeOfDay.TotalSeconds);
            return token;
        }

        private bool IsExpriedAccessToken(double createDate)
        {
            return createDate + _constants.EXPIRES_IN < DateTime.Now.TimeOfDay.TotalSeconds;
        }
    }
}
