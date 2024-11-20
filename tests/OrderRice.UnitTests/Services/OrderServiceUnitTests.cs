using Microsoft.Extensions.DependencyInjection;
using OrderLunch.Interfaces;

namespace OrderLunch.UnitTests.Services
{
    public class OrderServiceUnitTests : IClassFixture<DependencySetupFixture>
    {
        private readonly IOrderService _orderService;
        public OrderServiceUnitTests(DependencySetupFixture fixture)
        {
            using var scope = fixture.ServiceProvider.CreateScope();
            _orderService = scope.ServiceProvider.GetService<IOrderService>();
        }

        [Fact]
        public async Task CreateOrderListImage_ShouldReturnListImages()
        {
            (List<(string, string)> list, _, _) = await _orderService.CreateOrderListImage("list");

            Assert.NotNull(list);
            Assert.NotEmpty(list);
        }

        [Fact]
        public async Task OrderTicket_ShouldReturnTrue()
        {
            (bool isSuccess, _)  = await _orderService.OrderTicket();

            Assert.True(isSuccess);
        }

        [Fact]
        public async Task GenerateMessage_ShouldReturnValue()
        {
            List<string> users = ["user 01", "user 02", "user 03", "user 04", "user 05"];

            var message = await _orderService.GenerateMessageTakeTicket(users);

            Assert.NotNull(message);
            Assert.NotEmpty(message);
        }
    }
}
