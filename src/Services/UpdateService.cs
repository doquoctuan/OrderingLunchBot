using Microsoft.Extensions.Logging;
using OrderRice.Exceptions;
using OrderRice.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OrderRice.Services
{
    public class UpdateService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateService> _logger;
        private readonly IOrderService _orderService;

        public UpdateService(ITelegramBotClient botClient, ILogger<UpdateService> logger, IOrderService orderService)
        {
            _botClient = botClient;
            _logger = logger;
            _orderService = orderService;
        }

        public async Task HandleMessageAsync(Update update)
        {
            _logger.LogInformation("Invoke Telegram HandleMessageAsync function");

            if (update is null)
                return;

            var handler = update switch
            {
                { Message: { } message } => OnMessageReceived(message),
                _ => UnknownHandlerAsync(update),
            };

            await handler;
        }

        private async Task OnMessageReceived(Message message)
        {
            try
            {
                _logger.LogInformation("Received Message type: {MessageType} from {ChatId} ({FullName})", message.Type, message.Chat.Id, $"{message.From?.FirstName} {message.From?.LastName}");

                if (message.Text is not { } messageText)
                    return;

                var action = messageText.Split(' ')[0] switch
                {
                    "/list" or "/list@khaykhay_bot" => SendList(_botClient, _orderService, message),
                    _ => Task.CompletedTask
                };

                await action;
            }
            catch (OrderServiceException e) when (e.Message is { } messageException)
            {
                var errorMessage = messageException switch
                {
                    "Cannot find sheetId for the current month" => "Chưa tìm thấy sheet đặt phiếu ăn tháng này",
                    "Cannot find sheetId for the prev month" => "Không tìm thấy sheet đặt phiếu ăn tháng trư",
                    "Today, do not support registration for lunch" => "Hôm nay không hỗ trợ đặt phiếu ăn",
                    _ => "Hệ thống bận, vui lòng thử lại"
                };

                await _botClient.SendTextMessageAsync(message.Chat.Id, errorMessage);
            }

            #region LocalFunction

            static async Task SendList(ITelegramBotClient botClient, IOrderService _orderService, Message message)
            {
                (var response, string user16, string user19) = await _orderService.CreateOrderListImage();

                if (!response.Any())
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Không có đồng chí nào đặt phiếu ăn hôm nay");
                    return;
                }

                var albums = response.Select(x =>
                {
                    (string urlImage, string nameImage) = x;
                    return new InputMediaPhoto(InputFile.FromUri(urlImage))
                    {
                        Caption = nameImage
                    };
                });

                await botClient.SendMediaGroupAsync(message.Chat.Id, media: albums);
                if (message.Chat is { Username : "cronjob" })
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, string.IsNullOrEmpty(user16) ? "Tầng 16 không ai đặt phiếu ăn" : $"Đồng chí {user16} lấy phiếu cơm tầng 16");
                    await botClient.SendTextMessageAsync(message.Chat.Id, string.IsNullOrEmpty(user19) ? "Tầng 19 không ai đặt phiếu ăn" : $"Đồng chí {user19} lấy phiếu cơm tầng 19");
                }
            }

            #endregion

        }

        private Task UnknownHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update?.Type);
            return Task.CompletedTask;
        }
    }
}
