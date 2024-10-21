using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderLunch.Interfaces;
using OrderLunch.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OrderLunch.Functions
{
    public class TelegramFunction
    {
        private readonly ILogger<TelegramFunction> _logger;
        private readonly UpdateService _updateService;
        private readonly IOrderService _orderService;
        private const long DEVELOPMENT_DEPARMENT_ID = -1002094428444;
        private readonly ITelegramBotClient _botClient;

        public TelegramFunction(ILogger<TelegramFunction> logger, UpdateService updateService, IOrderService orderService,
            ITelegramBotClient botClient)
        {
            _logger = logger;
            _updateService = updateService;
            _orderService = orderService;
            _botClient = botClient;
        }

        [Function(nameof(AutoSendDebtorDaily))]
        public async Task AutoSendDebtorDaily([TimerTrigger("0 10 8 * * *")] TimerInfo timerInfo, FunctionContext context)
        {
            Update update = new()
            {
                Message = new() { Text = "/debtor", Chat = new() { Id = DEVELOPMENT_DEPARMENT_ID, Username = "cronjob" } }
            };
            await _updateService.HandleMessageAsync(update);
        }

        [Function(nameof(TelegramWebhook))]
        public async Task<IActionResult> TelegramWebhook([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
        {
            try
            {
                var body = await request.ReadAsStringAsync() ?? throw new ArgumentNullException(nameof(request));
                var update = JsonConvert.DeserializeObject<Update>(body);
                if (update is null)
                {
                    _logger.LogWarning("Unable to deserialize Update object.");
                    return new OkObjectResult("OK");
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

            return new OkObjectResult("OK");
        }
    }
}
