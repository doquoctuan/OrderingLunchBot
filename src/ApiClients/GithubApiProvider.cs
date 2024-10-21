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

        private async Task<T> Execute<T>(Func<Task<ApiResponse<T>>> func)
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

        private static MyApiException MapException(ApiException ex)
        {
            return ex.StatusCode switch
            {
                System.Net.HttpStatusCode.InternalServerError => new MyApiServerErrorException(ex),
                System.Net.HttpStatusCode.Forbidden => new MyApiForbiddenException(ex),
                // more cases..
                _ => new MyApiException(ex),
            };
        }
    }

    // Custom Exceptions
    public class MyApiException(ApiException ApiException) : Exception;
    public class MyApiForbiddenException(ApiException ApiException) : MyApiException(ApiException);
    public class MyApiServerErrorException(ApiException ApiException) : MyApiException(ApiException);
}
