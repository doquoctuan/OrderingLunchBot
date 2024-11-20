using Microsoft.Extensions.DependencyInjection;
using OrderLunch.ApiClients;

namespace OrderLunch.UnitTests.ApiClients
{
    public class BinanceApiClientTest : IClassFixture<DependencySetupFixture>
    {
        private readonly IBinanceApiClient binanceApiClient;

        public BinanceApiClientTest(DependencySetupFixture fixture)
        {
            using var scope = fixture.ServiceProvider.CreateScope();
            binanceApiClient = scope.ServiceProvider.GetService<IBinanceApiClient>();
        }

        [Fact]
        public async Task GetPriceBySymbol_ShouldReturnValue()
        {
            string symbol = "BTCUSDT";

            var priceSymbol = await binanceApiClient.GetPriceBySymbol(symbol);

            Assert.NotNull(priceSymbol);
            Assert.NotNull(priceSymbol.Symbol);
            Assert.NotEmpty(priceSymbol.Symbol);
        }
    }
}
