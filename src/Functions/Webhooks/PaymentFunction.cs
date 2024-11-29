using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderLunch.Entities;
using OrderLunch.Interfaces;
using OrderLunch.Models;
using OrderLunch.Persistence;
using OrderLunch.Services;
using Telegram.Bot;

namespace OrderLunch.Functions.Webhooks;

/// <summary>
/// Handles payment webhook requests and processes payment confirmations.
/// </summary>
/// <param name="googleSheetContext">The context for accessing Google Sheets data.</param>
/// <param name="updateService">The service for handling update messages.</param>
/// <param name="botClient">The Telegram bot client for sending messages.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response.</returns>
/// <exception cref="ArgumentNullException">Thrown when the request body is null.</exception>
/// <exception cref="NullReferenceException">Thrown when the user is not found in the Google Sheets context.</exception>
/// <exception cref="Exception">Thrown when an error occurs during the processing of the payment.</exception>
public class PaymentFunction(
    GoogleSheetContext googleSheetContext,
    UpdateService updateService,
    ITelegramBotClient botClient,
    IOrderService orderService,
    ILogger<PaymentFunction> logger
)
{
    /// <summary>
    /// The Telegram ID of the admin.
    /// </summary>
    private const long ADMIN_TELEGRAM_ID = 5664769574;

    [Function(nameof(PaymentFunction))]
    public async Task<IActionResult> Handler(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        try
        {
            var body = await request.ReadAsStringAsync() ?? throw new ArgumentNullException(nameof(request));
            var paymentWebhookDTO = JsonSerializer.Deserialize<PaymentWebhookDTO>(body) ?? throw new Exception("Deserialization failed.");

            var (isValidPayment, user) = await IsValidPayment(paymentWebhookDTO);

            if (isValidPayment)
            {
                if (user!.TelegramId is null or 0)
                {
                    throw new NullReferenceException(nameof(user.TelegramId));
                }

                // Dispatch event to UpdateService for confirming payment
                await updateService.HandleMessageAsync(new()
                {
                    Message = new()
                    {
                        Text = "/confirm",
                        Chat = new() { Id = user.TelegramId.Value, Username = user.UserName }
                    }
                });

                await botClient.SendTextMessageAsync(
                    user.TelegramId.Value,
                    $"Đã xác nhận thanh toán thành công cho <b>{user.FullName}</b>",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
        catch (NullReferenceException ex)
        {
            var requestBody = await request.ReadAsStringAsync();
            logger.LogError(ex, "Cannot find the user who made the payment:\n {RequestBody}", requestBody);
            await botClient.SendTextMessageAsync(ADMIN_TELEGRAM_ID, $"{ex.Message}\nRequestBody: {requestBody}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing the payment.");
        }

        return new OkResult();
    }

    private async Task<(bool, User)> IsValidPayment(PaymentWebhookDTO paymentDTO)
    {
        const string prefixPayment = "vts";
        const int LUNCH_TICKET_PRICE = 30_000;
        var additionalInfo = paymentDTO.Payment.Content.Split(" ");

        if (additionalInfo.Length <= 1 || !additionalInfo[0].Trim().StartsWith(prefixPayment, StringComparison.OrdinalIgnoreCase))
        {
            return (false, null);
        }

        var userName = additionalInfo[1].Trim().ToLower();
        var user = googleSheetContext.Users.FirstOrDefault(x => x.UserName.Equals(userName))
                   ?? throw new NullReferenceException(nameof(userName));

        var totalLunchOrder = await orderService.GetTotalLunchOrderByUser(user.FullName);
        var debtOfUser = totalLunchOrder * LUNCH_TICKET_PRICE;
        return (debtOfUser.Equals(paymentDTO.Payment.Amount), user);
    }
}
