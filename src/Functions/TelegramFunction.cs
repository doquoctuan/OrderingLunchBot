using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderRice.Interfaces;
using OrderRice.Services;
using System.Net;
using Telegram.Bot.Types;

namespace OrderRice.Functions
{
    public class TelegramFunction
    {
        private readonly ILogger<TelegramFunction> _logger;
        private readonly UpdateService _updateService;
        private readonly IOrderService _orderService;
        private const long DEVELOPMENT_DEPARMENT_ID = -1002094428444;

        public TelegramFunction(ILogger<TelegramFunction> logger, UpdateService updateService, IOrderService orderService)
        {
            _logger = logger;
            _updateService = updateService;
            _orderService = orderService;
        }

        [Function(nameof(AutoProtectedSheetDaily))]
        public async Task AutoProtectedSheetDaily([TimerTrigger("0 59 8 * * *")] TimerInfo timerInfo, FunctionContext context)
        {
            await _orderService.BlockOrderTicket();
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

        [Function(nameof(TelegramWebhook))]
        public async Task<HttpResponseData> TelegramWebhook([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var response = request.CreateResponse(HttpStatusCode.OK);
            try
            {
                var body = await request.ReadAsStringAsync() ?? throw new ArgumentNullException(nameof(request));
                var update = JsonConvert.DeserializeObject<Update>(body);
                if (update is null)
                {
                    _logger.LogWarning("Unable to deserialize Update object.");
                    return response;
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

            return response;
        }
    }
}
