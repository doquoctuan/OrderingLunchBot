using Microsoft.Extensions.DependencyInjection;
using OrderLunch.Interfaces;
using OrderLunch.Services;

namespace OrderLunch.UnitTests.Services
{
    public class UserServiceTests : IClassFixture<DependencySetupFixture>
    {
        private readonly IUserService _userService;

        public UserServiceTests(DependencySetupFixture fixture)
        {
            using var scope = fixture.ServiceProvider.CreateScope();
            _userService = scope.ServiceProvider.GetService<IUserService>();
        }

        [Fact]
        public void SearchUsers_ShouldReturnValue()
        {
            var res = _userService.Search("tuấn");
        }
    }
}
