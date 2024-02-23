using OrderRice.Interfaces;

namespace OrderRice.Services
{
    public class CachedGoogleAuthService : IGoogleAuthService
    {
        private readonly IGoogleAuthService _authService;

        public CachedGoogleAuthService(IGoogleAuthService authService)
        {
            _authService = authService;
        }

        public async Task<string> GetAccessTokenFromRefreshTokenAsync(CancellationToken cancellationToken)
        {
            var token = await _authService.GetAccessTokenFromRefreshTokenAsync(cancellationToken);
            return token;
        }
    }
}
