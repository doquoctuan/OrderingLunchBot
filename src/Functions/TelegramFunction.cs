using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderRice.Interfaces;
using OrderRice.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OrderRice.Functions
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

        [Function(nameof(AutoDisciplineReminder))]
        public async Task AutoDisciplineReminder([TimerTrigger("0 0 17 * * 1-5")] TimerInfo timerInfo, FunctionContext context)
        {
            await _botClient.SendTextMessageAsync(chatId: DEVELOPMENT_DEPARMENT_ID, $@"
                                ## Thông Báo Nhắc Nhở
                                #### Xin chào các anh/chị,
                                
                                Trước khi ra về, vui lòng thực hiện các công việc sau:
                                
                                1. Khai báo và cập nhật CV trên Jira.
                                2. Sắp xếp lại ghế ngồi gọn gàng.
                                3. Vệ sinh khu vực làm việc.
                                4. Tắt màn hình máy tính.
                                
                                Xin cảm ơn sự hợp tác của các **anh/chị**", parseMode: ParseMode.MarkdownV2);
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
