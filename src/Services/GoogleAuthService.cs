using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrderLunch;
using OrderLunch.Helper;
using OrderLunch.Interfaces;
using OrderLunch.ResponseModels;

namespace OrderLunch.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly Constants _constants;

        public GoogleAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration, Constants constants)
        {
            _client = httpClientFactory.CreateClient("google_auth_client");
            _configuration = configuration;
            _constants = constants;
        }

        public async Task<string> GetAccessTokenFromRefreshTokenAsync(CancellationToken cancellationToken)
        {
            // Get the authorization server endpoint and credentials from configuration
            var tokenEndpoint = _configuration["Google_TokenEndpoint"];
            var clientId = _configuration["Google_ClientId"];
            var clientSecret = _configuration["Google_ClientSecret"];
            var refreshToken = _configuration["Google_RefreshToken"];

            var objQueryString = new
            {
                grant_type = "refresh_token",
                client_id = clientId,
                client_secret = clientSecret,
                refresh_token = refreshToken,
            };

            // Send the token request and get the response
            string queryString = StringUtils.ToQueryStringUsingNewtonsoftJson(objQueryString);
            var response = await _client.PostAsync($"{tokenEndpoint}?{queryString}", null, cancellationToken);

            // Ensure the response is successful
            response.EnsureSuccessStatusCode();

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
                },
                Formatting = Formatting.Indented
            };

            // Parse the access token from the response
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonConvert.DeserializeObject<GoogleAuthResponseModel>(content, settings);

            _constants.EXPIRES_IN = tokenResponse.ExpiresIn;
            return tokenResponse.AccessToken;

        }

    }
}
