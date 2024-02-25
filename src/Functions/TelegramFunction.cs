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
        private readonly UpdateService _updateService;

        public TelegramFunction(ILogger<TelegramFunction> logger, UpdateService updateService)
        {
            _logger = logger;
            _updateService = updateService;
        }

        [Function(nameof(ManualTrigger))]
        public async Task<HttpResponseData> ManualTrigger([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            var chatId = request.Query["chatId"];
            if (string.IsNullOrEmpty(chatId) || !long.TryParse(chatId, out long parseLongId))
            {
                return request.CreateResponse(HttpStatusCode.BadRequest);
            }
            Update update = new()
            {
                Message = new() { Text = "/list", Chat = new() { Id = parseLongId, Username = "cronjob" } }
            };
            await _updateService.HandleMessageAsync(update);
            return response;
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
                if (update is null)
                {
                    _logger.LogWarning("Unable to deserialize Update object.");
                    return response;
                }

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
