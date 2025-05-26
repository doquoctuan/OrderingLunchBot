using Microsoft.Extensions.DependencyInjection;
using OrderLunch.ApiClients;

namespace OrderLunch.UnitTests.ApiClients
{
    public class WhapiClientTest : IClassFixture<DependencySetupFixture>
    {
        private readonly IWhapiClient whapiClient;

        public WhapiClientTest(DependencySetupFixture fixture)
        {
            using var scope = fixture.ServiceProvider.CreateScope();
            whapiClient = scope.ServiceProvider.GetService<IWhapiClient>();
        }

        [Fact]
        public async Task SendMessages_ShouldReturnOK()
        {
            var response = await whapiClient.SendMessages(new MessagePayload
            {
                Body = "This is a test message",
                To = "120363419971888833"
            });

            Assert.NotNull(response);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendMediaMessages_ShouldReturnOK()
        {
            var response = await whapiClient.SendMediaMessages(new SendMediaMsgPayload
            {
                Caption = "This is a test message",
                To = "120363419971888833",
                Media = "https://raw.githubusercontent.com/doquoctuan/OrderingLunchImage/26a2acbdb79621e409088a64c0fec890ac4be14e/list/list_26052025_638838486204376179.png"
            });

            Assert.NotNull(response);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
