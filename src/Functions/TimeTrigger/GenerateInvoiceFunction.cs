using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderLunch.Entities;
using OrderLunch.Interfaces;
using OrderLunch.Persistence;
using OrderLunch.Services;
using Telegram.Bot;

namespace OrderLunch.Functions.TimeTrigger;

public class GenerateInvoiceFunction(
        ILogger<GenerateInvoiceFunction> logger,
        GoogleSheetContext googleSheetContext,
        UpdateService updateService,
        ITelegramBotClient botClient,
        IOrderService orderService
    )
{
    private const long ADMIN_TELEGRAM_ID = 5664769574;
    private const int BATCH_SIZE = 5;

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
