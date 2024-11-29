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
                if (user is null || user!.TelegramId is null or 0)
                {
                    throw new NullReferenceException($"User not found for payment: {JsonSerializer.Serialize(paymentWebhookDTO)}");
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
            }
        }
        catch (NullReferenceException ex)
        {
            await botClient.SendTextMessageAsync(ADMIN_TELEGRAM_ID, $"{ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing the payment.");
        }

        return new OkResult();
    }

    /// <summary>
    /// Validates the payment information from the webhook and checks if it matches the user's debt.
    /// </summary>
    /// <param name="paymentDTO">The payment data transfer object containing payment details.</param>
    /// <returns>
    /// A tuple where the first item is a boolean indicating whether the payment is valid,
    /// and the second item is the user associated with the payment if valid; otherwise, null.
    /// </returns>
    private async Task<(bool, User)> IsValidPayment(PaymentWebhookDTO paymentDTO)
    {
        // Extract the string from "vts" to the end of the string
        var startIndex = paymentDTO.Payment.Content.IndexOf("vts", StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
        {
            return (false, null);
        }
        var extractedString = paymentDTO.Payment.Content[startIndex..];
        const string PrefixPayment = "vts";
        const int LunchTicketPrice = 30_000;
        var paymentDetails = extractedString.Split(" ");

        if (paymentDetails.Length <= 1 || !paymentDetails[0].Trim().StartsWith(PrefixPayment, StringComparison.OrdinalIgnoreCase))
        {
            return (false, null);
        }

        var userName = paymentDetails[1].Trim().ToLower();
        var user = googleSheetContext.Users.FirstOrDefault(x => x.UserName.Equals(userName));
        if (user == null)
        {
            return (true, null);
        }

        var totalLunchOrders = await orderService.GetTotalLunchOrderByUser(user.FullName);
        var userDebt = totalLunchOrders * LunchTicketPrice;
        return (userDebt == paymentDTO.Payment.Amount, user);
    }
}
