using Microsoft.Extensions.Logging;
using OrderRice.Interfaces;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OrderRice.Services
{
    public class TelegramService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramService> _logger;
        private readonly IOrderService _orderService;

        public TelegramService(ITelegramBotClient botClient, ILogger<TelegramService> logger, IOrderService orderService)
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
            _logger.LogInformation("Received Message type: {MessageType} from {ChatId} ({FullName})", message.Type, message.Chat.Id, $"{message.From?.FirstName} {message.From?.LastName}");

            if (message.Text is not { } messageText)
                return;

            var action = messageText.Split(' ')[0] switch
            {
                "/list" or "/list@khay_bot" => SendList(_botClient, _orderService, message),
                _ => Task.CompletedTask
            };

            await action;

            static async Task SendList(ITelegramBotClient botClient, IOrderService _orderService, Message message)
            {
                var response = await _orderService.CreateOrderListImage();

                if (!response.Any())
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Không tìm thấy danh sách đăng ký cơm hôm nay");
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
            }
        }

        private Task UnknownHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update?.Type);
            return Task.CompletedTask;
        }
    }
}
