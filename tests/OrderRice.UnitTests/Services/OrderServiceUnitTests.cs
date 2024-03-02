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

        //[Fact]
        //public async Task CreateOrderListImage_ShouldReturnUrl()
        //{
        //    // Action
        //    var result = await _orderService.CreateOrderListImage();

        //    // Assert
        //    Assert.NotEmpty(result);
        //}

        [Fact]
        public async Task OrderUnitTest()
        {
            await _orderService.Order("cuongvt13", DateTime.Now.AddDays(5), isOrder: false, isAll: false);
            await _orderService.Order("quandm7", DateTime.Now.AddDays(5), isOrder: false, isAll: false);
            await _orderService.Order("thinnt1", DateTime.Now.AddDays(5), isOrder: false, isAll: false);
            await _orderService.Order("anpb7", DateTime.Now.AddDays(5), isOrder: false, isAll: false);
            await _orderService.Order("quihp1", DateTime.Now.AddDays(5), isOrder: false, isAll: false);
        }
    }
}
