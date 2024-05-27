using Refit;

namespace OrderRice.ApiClients
{
    public interface IGithubApiClient
    {
        [Get("/users/{user}")]
        Task<ApiResponse<UserResponseItem>> GetUser(string user);
    }
    public record class UserResponseItem(Guid Id, string Name);
}
