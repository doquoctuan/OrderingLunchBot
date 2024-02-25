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

        //[Fact]
        //public async Task UploadImage_ShouldReturnSuccess()
        //{
        //    // Action
        //    var result = await _githubService.UploadImageAsync(imageBase64: "test");

        //    // Assert
        //    Assert.NotNull(result.Content?.DownloadUrl);
        //    Assert.NotEmpty(result.Content?.DownloadUrl);
        //}
    }
}
