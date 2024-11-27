using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderLunch.Entities;
using OrderLunch.Exceptions;
using OrderLunch.Helper;
using OrderLunch.Interfaces;
using OrderLunch.Persistence;
using OrderLunch.Validations;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;
using User = OrderLunch.Entities.User;

namespace OrderLunch.Services
{
    public class UpdateService
    {
        private readonly string SPREADSHEET_ID;
        private readonly string SHEET_NAME;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateService> _logger;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly SpreadsheetsResource.ValuesResource _googleSheetValues;
        private readonly GoogleSheetContext _googleSheetContext;
        private readonly string urlPaymentInfo;

        public UpdateService(
            ITelegramBotClient botClient,
            ILogger<UpdateService> logger,
            IOrderService orderService,
            GoogleSheetsHelper googleSheetsHelper,
            GoogleSheetContext googleSheetContext,
            IUserService userService,
            IPaymentService paymentService,
            IConfiguration configuration)
        {
            _botClient = botClient;
            _logger = logger;
            _orderService = orderService;
            _userService = userService;
            _paymentService = paymentService;
            _googleSheetValues = googleSheetsHelper.Service.Spreadsheets.Values;
            _googleSheetContext = googleSheetContext;
            SPREADSHEET_ID = configuration["GoogleSheetDatasource"];
            SHEET_NAME = configuration["GoogleSheetName"];
            urlPaymentInfo = configuration["BASE_IMAGE_PAYMENTINFO"];
        }

        public async Task HandleMessageAsync(Update update)
        {
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

                (string command, string text) = SeparateTelegramMessage(messageText);

                (bool isExist, Entities.User user) = IsExists(message.Chat.Id, text);

                if (!isExist && message.Chat is not null && message.Chat.Username is not null && !message.Chat.Username.Equals("cronjob"))
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Để bắt đầu, vui lòng cho biết bạn là ai\nSử dụng lệnh /set {username}");
                    return;
                }

                var action = command switch
                {
                    "search" or "s" or "s@khaykhay_bot" or "search@khaykhay_bot" => SearchHandler(_botClient, _userService, message, text),
                    "list" or "list@khaykhay_bot" => SendList(_botClient, _orderService, message),
                    "debtor" or "debtor@khaykhay_bot" => SendDebtor(_botClient, _orderService, message),
                    "menu" or "menu@khaykhay_bot" => SendMenu(_botClient, _orderService, message),
                    "order" or "order@khaykhay_bot" => Order(_botClient, _orderService, message, text, user, isOrder: true, isAll: false),
                    "unorder" or "unorder@khaykhay_bot" => Order(_botClient, _orderService, message, text, user, isOrder: false, isAll: false),
                    "orderall" or "orderall@khaykhay_bot" => Order(_botClient, _orderService, message, text, user, isOrder: true, isAll: true),
                    "unorderall" or "unorderall@khaykhay_bot" => Order(_botClient, _orderService, message, text, user, isOrder: false, isAll: true),
                    "set" or "set@khaykhay_bot" => SetTelegramId(_botClient, _googleSheetValues, user, message.Chat.Id),
                    "confirm" or "confirm@khaykhay_bot" => PaymentConfirmation(_botClient, user, message.Chat.Id),
                    "pay" or "pay@khaykhay_bot" => GeneratePaymentLink(_botClient, user, message.Chat.Id),
                    _ => Task.CompletedTask
                };

                await action;
            }
            catch (OrderServiceException e) when (e.Message is { } messageException)
            {
                var errorMessage = messageException switch
                {
                    "Cannot find sheetId for the current month" => "Chưa tìm thấy sheet đặt phiếu ăn tháng này",
                    "Cannot find sheetId for the prev month" => "Không tìm thấy sheet đặt phiếu ăn tháng vừa rồi",
                    "Today, do not support registration for lunch" => "Hôm nay không hỗ trợ đặt phiếu ăn",
                    "The user does not exists" => "Không tìm thấy người dùng này, vui lòng liên hệ tuandq16 (0367717714) để thêm mới",
                    _ => "Hệ thống bận, vui lòng thử lại"
                };

                await _botClient.SendTextMessageAsync(message.Chat.Id, errorMessage);
            }

            static async Task SearchHandler(ITelegramBotClient botClient, IUserService userService, Message message, string keyword)
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, text: "Vui lòng nhập từ khóa để tìm kiếm");
                    return;
                }

                var users = await userService.Search(keyword);

                if (users.Count == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, text: "Không tìm thấy thông tin người dùng");
                    return;
                }

                foreach (var user in users)
                {
                    try
                    {
                        await botClient.SendPhotoAsync(
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        chatId: message.Chat.Id,
                        photo: InputFile.FromUri(user.Avatar),
                        caption: $"Họ tên: <b>{user.StaffName}</b>\nUsername: <b>{user.StaffCode}</b>\nDi động: <b>{user.PhoneNumber}</b>\nEmail: <b>{user.Email}</b>"
                        );
                    }
                    catch (Exception)
                    {
                        await botClient.SendPhotoAsync(
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        chatId: message.Chat.Id,
                        photo: InputFile.FromUri("https://play-lh.googleusercontent.com/KDIgP3EBaj_aomyPqAFkt_gbHSoNWQPPLywutWXw1hoM-0lqGsQ2zHZW0GVuSpRVA0g"),
                        caption: $"Họ tên: <b>{user.StaffName}</b>\nUsername: <b>{user.StaffCode}</b>\nDi động: <b>{user.PhoneNumber}</b>\nEmail: <b>{user.Email}</b>"
                        );
                    }
                }
            }

            static async Task SendList(ITelegramBotClient botClient, IOrderService _orderService, Message message)
            {
                (var response, string assignedMessage, string user19) = await _orderService.CreateOrderListImage(message?.From?.Username ?? "list");

                if (!response.Any())
                {
                    if (message?.Chat is not { Username: "cronjob" })
                    {
                        await botClient.SendTextMessageAsync(message?.Chat.Id, text: $"Không có đồng chí nào đặt phiếu ăn hôm nay");
                    }
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

                await botClient.SendMediaGroupAsync(message?.Chat.Id, media: albums);

                if (message?.Chat is { Username: "cronjob" })
                {
                    if (!string.IsNullOrEmpty(assignedMessage))
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, assignedMessage, parseMode: ParseMode.Html);
                    }
                }
            }

            async Task SendDebtor(ITelegramBotClient botClient, IOrderService _orderService, Message message)
            {
                var images = await _orderService.CreateUnpaidImage();

                if (!images.Any())
                {
                    if (!message.Chat.Username.Equals("cronjob"))
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, text: $"Đã thu hồi xong nợ");
                    }
                    return;
                }

                var albums = images.Select(x =>
                {
                    (string base64Img, string nameImage) = x;
                    var bytes = Convert.FromBase64String(base64Img);
                    return new InputMediaPhoto(InputFile.FromStream(new MemoryStream(bytes), fileName: $"{nameImage}.png"))
                    {
                        Caption = nameImage,
                    };
                });

                await botClient.SendMediaGroupAsync(message.Chat.Id, media: albums);
                await botClient.SendPhotoAsync(message.Chat.Id, photo: InputFile.FromUri(urlPaymentInfo));
            }

            static async Task SendMenu(ITelegramBotClient botClient, IOrderService _orderService, Message message)
            {
                var dateNow = DateTime.Now;
                StringBuilder messageText = new();
                var menu = await _orderService.GetMenu(dateNow);

                if (!menu.Any())
                {
                    if (message.Chat.Username.Equals("cronjob"))
                    {
                        return;
                    }
                    messageText.Append($"Thứ {(int)dateNow.DayOfWeek + 1} Ngày {dateNow:dd/MM/yyyy} chưa có thực đơn");
                }
                else
                {
                    messageText.Append($"Thực đơn thứ {(int)dateNow.DayOfWeek + 1} ngày {dateNow:dd/MM/yyyy}:\n");
                    foreach (var item in menu.Keys)
                    {
                        messageText.Append($"{item.Trim()}\n");
                    }
                }

                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: messageText.ToString());
            }

            static async Task Order(ITelegramBotClient botClient, IOrderService _orderService, Message message, string text, Entities.User user, bool isAll = false, bool isOrder = true)
            {
                var orderValidator = OrderValidation.CreateValidator(DateTime.Now);

                var isValid = orderValidator(user.UserName);

                if (!isValid)
                {
                    throw new OrderServiceException("Cannot order lunch today.");
                }

                string userName = string.IsNullOrEmpty(text) ? user.UserName : text;

                StringBuilder messageText = new();
                string operation = isOrder ? "Đặt cơm" : "Huỷ cơm";
                messageText.Append(operation);

                var isSucess = await _orderService.Order(
                        userName: userName,
                        dateTime: DateTime.Now,
                        isOrder: isOrder,
                        isAll: isAll
                    );

                if (isSucess)
                {
                    messageText.Append(" thành công");
                }
                else messageText.Append(" thất bại, sheet cơm đã bị khóa");

                messageText.Append($" cho đồng chí {userName}");

                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: messageText.ToString());
            }

            async Task SetTelegramId(ITelegramBotClient botClient, SpreadsheetsResource.ValuesResource _googleSheetValues, Entities.User user, long chatId)
            {
                var range = $"{SHEET_NAME}!J{user.RowNum}:J{user.RowNum}";
                var valueRange = new ValueRange
                {
                    Values = new List<IList<object>> { new List<object>() { chatId } }
                };
                var updateRequest = _googleSheetValues.Update(valueRange, SPREADSHEET_ID, range);
                updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
                updateRequest.Execute();

                await botClient.SendTextMessageAsync(chatId, text: $"Cập nhật thông tin thành công cho đồng chí {user.FullName}");
            }

            async Task PaymentConfirmation(ITelegramBotClient botClient, User user, long chatId)
            {
                bool isSuccess = await _orderService.PaymentConfirmation(user?.FullName);
                string messageText = isSuccess ? $"Đã xác nhận thanh toán tiền cơm tháng {DateTime.Now.AddMonths(-1).Month} cho đồng chí {user?.FullName}" : "Chưa thể xác nhận thanh toán vào lúc này, vui lòng thử lại.";
                await botClient.SendTextMessageAsync(chatId, text: messageText);
            }

            async Task GeneratePaymentLink(ITelegramBotClient botClient, User user, long chatId)
            {
                var totalLunchOrder = await _orderService.GetTotalLunchOrderByUser(user?.FullName);
                var paymentLink = await _paymentService.GeneratePaymentLinkAsync(totalLunchOrder, user.UserName);
                StringBuilder paymentInfoMessage = new();
                paymentInfoMessage.Append($"<b>Hoá đơn tiền cơm tháng {DateTime.Now.Month - 1}</b>");
                paymentInfoMessage.AppendLine($"Họ tên: <b>{user?.FullName}</b>");
                paymentInfoMessage.AppendLine($"Số lượng phiếu: <b>{totalLunchOrder}</b>");
                paymentInfoMessage.AppendLine($"Tổng tiền: <b>{totalLunchOrder * 30.000}</b>");
                paymentInfoMessage.AppendLine($"Vui lòng quét mã QR để thanh toán</b>");
                await botClient.SendPhotoAsync(
                    chatId: chatId, 
                    photo: InputFile.FromUri(paymentLink), 
                    parseMode: ParseMode.Html, 
                    caption: paymentInfoMessage.ToString()
                );
            }
        }

        private Task UnknownHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update?.Type);
            return Task.CompletedTask;
        }

        private (bool, User) IsExists(long telegramId, string messageText)
        {
            var users = _googleSheetContext
                            .Users
                            .Where(x => x.UserName.Equals(messageText) || x.TelegramId.Equals(telegramId)).ToList();

            if (users is null || !users.Any())
            {
                return new(false, null);
            }

            if (users.Any(x => x.UserName.Equals(messageText)))
            {
                return new(true, users.Find(x => x.UserName.Equals(messageText)));
            }

            return new(true, users[0]);
        }

        private (string, string) SeparateTelegramMessage(string telegramMessage)
        {
            string pattern = @"^/(\w+)\s*(.*)$";
            Regex regex = new(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(1000));
            Match match = regex.Match(telegramMessage);
            if (match.Success)
            {
                return new(match.Groups[1].Value, match.Groups[2].Value);
            }
            return new(string.Empty, telegramMessage.Trim());
        }

    }
}
