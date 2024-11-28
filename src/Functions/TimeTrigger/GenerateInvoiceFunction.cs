using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderLunch.Entities;
using OrderLunch.Interfaces;
using OrderLunch.Persistence;
using OrderLunch.Services;
using Telegram.Bot;

namespace OrderLunch.Functions.TimeTrigger;

/// <summary>
/// Represents a function that generates invoices for unpaid orders.
/// </summary>
/// <param name="logger">The logger instance used for logging information and errors.</param>
/// <param name="googleSheetContext">The context for accessing Google Sheets data.</param>
/// <param name="updateService">The service used to handle updates and messages.</param>
/// <param name="botClient">The Telegram bot client used for sending messages.</param>
/// <param name="orderService">The service used to manage orders.</param>
public class GenerateInvoiceFunction(
        ILogger<GenerateInvoiceFunction> logger,
        GoogleSheetContext googleSheetContext,
        UpdateService updateService,
        ITelegramBotClient botClient,
        IOrderService orderService
    )
{
    /// <summary>
    /// The Telegram ID of the admin.
    /// </summary>
    private const long ADMIN_TELEGRAM_ID = 5664769574;

    /// <summary>
    /// The size of the batch for processing unpaid users.
    /// </summary>
    private const int BATCH_SIZE = 5;

    /// <summary>
    /// Handles the timer trigger to generate invoices.
    /// </summary>
    /// <param name="timerInfo">The timer information.</param>
    /// <param name="context">The function context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Function(nameof(GenerateInvoiceFunction))]
    public async Task Handler([TimerTrigger("0 0 8 * * *")] TimerInfo timerInfo, FunctionContext context)
    {
        try
        {
            logger.LogInformation("GenerateInvoiceFunction executed at: {time}", DateTime.Now);

            var unPaidMap = await orderService.UnPaidList();
            var unPaidList = unPaidMap.Select(x => x.Value.Item1).ToHashSet();
            var users = googleSheetContext.Users;

            var tasks = users.Where(x => unPaidList.Contains(x.FullName)).Select(ProcessUnpaidUser);

            logger.LogInformation("Start processing with batching at: {time}", DateTime.Now);

            var taskList = tasks.ToList();
            foreach (var batch in taskList.Chunk(BATCH_SIZE))
            {
                await Task.WhenAll(batch);
            }

            logger.LogInformation("GenerateInvoiceFunction completed at: {time}", DateTime.Now);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while generating invoices.");
            throw;
        }
    }

    /// <summary>
    /// Processes an unpaid user by sending a payment reminder or notifying the admin.
    /// </summary>
    /// <param name="unPaidUser">The unpaid user to process.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ProcessUnpaidUser(User unPaidUser)
    {
        if (unPaidUser.TelegramId > 0)
        {
            await updateService.HandleMessageAsync(new()
            {
                Message = new()
                {
                    Text = "/pay",
                    Chat = new() { Id = unPaidUser.TelegramId.Value, Username = unPaidUser.UserName }
                }
            });
        }
        else
        {
            await botClient.SendTextMessageAsync(ADMIN_TELEGRAM_ID, $"{unPaidUser.FullName} has not linked their telegram account");
        }
    }
}
