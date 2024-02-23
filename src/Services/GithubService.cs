using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OrderRice.ResponseModels;
using System.Net.Http.Json;

namespace OrderRice.Services
{
    public class GithubService
    {
        private readonly HttpClient _httpClient;
        private readonly string DEFAULT_REPOSITORY_URL;
        private const string DEFAULT_BRANCH = "images";
        public GithubService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("github_client");
            DEFAULT_REPOSITORY_URL = $"repos/{configuration["GITHUB_REPOSITORY_NAME"]}/contents";
        }

        public async Task<GitResponseModel> UploadImageAsync(string imageBase64, string folderSource = "default", string prefixName = "unknown")
        {
            var fileName = $"{prefixName}_{DateTime.Now:ddMMyyyy}_{DateTime.Now.Ticks}.png";
            var commit = $"{prefixName}_{DateTime.Now:dd_MM_yyyy hh:mm tt}";

            var jsonBody = new
            {
                message = commit,
                branch = DEFAULT_BRANCH,
                content = imageBase64
            };

            var response = await _httpClient.PutAsJsonAsync($"{DEFAULT_REPOSITORY_URL}/{folderSource}/{fileName}", jsonBody);
            var reponseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GitResponseModel>(reponseString.Replace("download_url", "downloadUrl"));
        }
    }
}
