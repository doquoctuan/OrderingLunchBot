﻿using Microsoft.AspNetCore.Mvc;
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

        [Function(nameof(AutoSendDebtorDaily))]
        public async Task AutoSendDebtorDaily([TimerTrigger("0 10 8 * * *")] TimerInfo timerInfo, FunctionContext context)
        {
            Update update = new()
            {
                Message = new() { Text = "/debtor", Chat = new() { Id = DEVELOPMENT_DEPARMENT_ID, Username = "cronjob" } }
            };
            await _updateService.HandleMessageAsync(update);
        }

        [Function(nameof(AutoDisciplineReminder))]
        public async Task AutoDisciplineReminder([TimerTrigger("0 0 17 * * 1-5")] TimerInfo timerInfo, FunctionContext context)
        {
            await _botClient.SendTextMessageAsync(chatId: DEVELOPMENT_DEPARMENT_ID, $@"
<b>Thông Báo Nhắc Nhở</b>

Xin chào các <b>đồng chí,</b>

Trước khi ra về, vui lòng thực hiện các công việc sau:
<b>1. Khai báo và cập nhật CV trên Jira.</b>
<b>2. Sắp xếp lại ghế ngồi gọn gàng.</b>
<b>3. Vệ sinh khu vực làm việc.</b>
<b>4. Tắt màn hình máy tính.</b>

Xin cảm ơn sự hợp tác của các <b>đồng chí</b>", parseMode: ParseMode.Html);
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
