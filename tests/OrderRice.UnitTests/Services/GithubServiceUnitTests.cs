using Microsoft.Extensions.DependencyInjection;
using OrderRice.Services;

namespace OrderRice.UnitTests.Services
{
    public class GithubServiceUnitTests : IClassFixture<DependencySetupFixture>
    {
        private readonly GithubService _githubService;
        public GithubServiceUnitTests(DependencySetupFixture fixture)
        {
            // Arrange
            using var scope = fixture.ServiceProvider.CreateScope();
            _githubService = scope.ServiceProvider.GetService<GithubService>();
        }

        [Fact]
        public async Task UploadImage_ShouldReturnImageUrl()
        {
            // Arrange
            string imageUrl = "https://viettelfamily.com/uploads/viettelfamily/360/kt11/logo-vtf2-1.png";
            using HttpClient client = new();
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
            string base64String = Convert.ToBase64String(imageBytes);

            // Action
            var result = await _githubService.UploadImageAsync(imageBase64: base64String, folderSource: "unitTest", prefixName: "unitTest");

            // Assert
            Assert.NotNull(result.Content?.DownloadUrl);
            Assert.NotEmpty(result.Content?.DownloadUrl);
        }
    }
}
