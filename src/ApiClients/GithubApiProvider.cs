using Refit;

namespace OrderLunch.ApiClients
{
    public class GithubApiProvider(IGithubApiClient githubApiClient)
    {
        private readonly IGithubApiClient _githubApiClient = githubApiClient;

        public Task<UserResponseItem> GetUser(string user)
        {
            return Execute(() => _githubApiClient.GetUser(user));
        }

        private static async Task<T> Execute<T>(Func<Task<ApiResponse<T>>> func)
        {
            ApiResponse<T> response;

            try
            {
                response = await func.Invoke().ConfigureAwait(false);
            }
            catch (ApiException ex)
            {
                throw MapException(ex);
            }

            return response.Content;
        }

        private static GithubApiException MapException(ApiException ex)
        {
            return ex.StatusCode switch
            {
                System.Net.HttpStatusCode.InternalServerError => new GithubApiServerErrorException(ex),
                System.Net.HttpStatusCode.Forbidden => new GithubApiForbiddenException(ex),
                // more cases..
                _ => new GithubApiException(ex),
            };
        }
    }

    // Custom Exceptions
    public class GithubApiException(ApiException ApiException) : Exception
    {
        private readonly ApiException ApiException = ApiException;
    }

    public class GithubApiForbiddenException(ApiException ApiException) : GithubApiException(ApiException);
    public class GithubApiServerErrorException(ApiException ApiException) : GithubApiException(ApiException);
}
