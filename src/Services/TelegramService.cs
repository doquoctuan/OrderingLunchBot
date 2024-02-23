using Microsoft.Extensions.Logging;
using OrderRice.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

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
            await _orderService.CreateOrderListImage();
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

            await _botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: message.Text);
        }

        private Task UnknownHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update?.Type);
            return Task.CompletedTask;
        }
    }
}
