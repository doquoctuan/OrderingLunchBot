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
    public class TelegramFunction(
        ILogger<TelegramFunction> logger,
        UpdateService updateService,
        IOrderService orderService,
        ITelegramBotClient botClient)
    {
        private readonly ILogger<TelegramFunction> _logger = logger;
        private readonly UpdateService _updateService = updateService;
        private readonly IOrderService _orderService = orderService;
        private const long DEVELOPMENT_DEPARMENT_ID = -1002094428444;
        private readonly ITelegramBotClient _botClient = botClient;

        [Function(nameof(TelegramWebhook))]
        public async Task<IActionResult> TelegramWebhook([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
        {
            Update update = null;
            try
            {
                var body = await request.ReadAsStringAsync() ?? throw new ArgumentNullException(nameof(request));
                update = JsonConvert.DeserializeObject<Update>(body);
                if (update is null)
                {
                    _logger.LogError("Unable to deserialize Update object.");
                    return new OkObjectResult("OK");
                }

                await _updateService.HandleMessageAsync(update);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: {Message}\n {StackTrace}", e.Message, e.StackTrace);

                if (update is not null)
                {
                    await _botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id, "Hệ thống bận, vui lòng thử lại sau");
                }
            }

            return new OkObjectResult("OK");
        }

        [Function(nameof(AutoProtectedSheetDaily))]
        public async Task AutoProtectedSheetDaily([TimerTrigger("0 59 8 * * *")] TimerInfo timerInfo, FunctionContext context)
        {
            await _orderService.BlockOrderTicket();
        }

        [Function(nameof(AutoOrderTicketDaily))]
        public async Task AutoOrderTicketDaily([TimerTrigger("0 0 9 * * 1-5")] TimerInfo timerInfo, FunctionContext context)
        {
            try
            {
                (bool isSucess, int total) = await _orderService.OrderTicket();
                if (isSucess)
                {
                    await _botClient.SendTextMessageAsync(chatId: DEVELOPMENT_DEPARMENT_ID, $"Khầy đã đặt cơm cho {total} đồng chí");
                }
                else throw new Exception("Không thể đặt cơm cho phòng Phát triển");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                await _botClient.SendTextMessageAsync(chatId: DEVELOPMENT_DEPARMENT_ID, ex.Message);
            }
        }

        [Function(nameof(AutoSendListDaily))]
        public async Task AutoSendListDaily([TimerTrigger("0 30 9 * * *")] TimerInfo timerInfo, FunctionContext context)
        {
            Update update = new()
            {
                Message = new() { Text = "/list", Chat = new() { Id = DEVELOPMENT_DEPARMENT_ID, Username = "cronjob" } }
            };
            await _updateService.HandleMessageAsync(update);
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

        [Function(nameof(AutoSendMenuDaily))]
        public async Task AutoSendMenuDaily([TimerTrigger("0 0 8 * * 1-5")] TimerInfo timerInfo, FunctionContext context)
        {
            Update update = new()
            {
                Message = new() { Text = "/menu", Chat = new() { Id = DEVELOPMENT_DEPARMENT_ID, Username = "cronjob" } }
            };
            await _updateService.HandleMessageAsync(update);
        }
    }
}
