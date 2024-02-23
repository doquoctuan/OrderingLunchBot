using OrderRice.Interfaces;
using System.Net.Http.Headers;

namespace OrderRice
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IGoogleAuthService _authService;
        public AuthTokenHandler(IGoogleAuthService authService)
            => _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Call the authentication service to get an access token
            var accessToken = await _authService.GetAccessTokenFromRefreshTokenAsync(cancellationToken);

            // Set the authorization header with the access token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
