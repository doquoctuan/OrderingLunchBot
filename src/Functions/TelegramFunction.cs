using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderRice.Services;
using System.Net;
using Telegram.Bot.Types;

namespace OrderRice.Functions
{
    public class TelegramFunction
    {
        private readonly ILogger<TelegramFunction> _logger;
        private readonly TelegramService _updateService;

        public TelegramFunction(ILogger<TelegramFunction> logger, TelegramService updateService)
        {
            _logger = logger;
            _updateService = updateService;
        }

        [Function(nameof(TelegramWebhook))]
        public async Task<HttpResponseData> TelegramWebhook([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var response = request.CreateResponse(HttpStatusCode.OK);
            try
            {
                var body = await request.ReadAsStringAsync() ?? throw new ArgumentNullException(nameof(request));
                var update = JsonConvert.DeserializeObject<Update>(body);
                //if (update is null)
                //{
                //    _logger.LogWarning("Unable to deserialize Update object.");
                //    return response;
                //}

                await _updateService.HandleMessageAsync(update);
            }
            catch (ArgumentNullException)
            {
                _logger.LogError("Not Telegram calling webhook");
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: {Message}", e.Message);
            }

            return response;
        }
    }
}
