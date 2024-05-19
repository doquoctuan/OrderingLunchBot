using Microsoft.Extensions.DependencyInjection;
using OrderRice.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRice.UnitTests.Services
{
    public class OrderServiceUnitTests : IClassFixture<DependencySetupFixture>
    {
        private readonly IOrderService _orderService;
        public OrderServiceUnitTests(DependencySetupFixture fixture)
        {
            // Arrange
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
            await _orderService.OrderTicket();
        }
    }
}
