using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace OrderLunch.Middlewares
{
    public class TelegramAuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly string _secretToken;
        public TelegramAuthenticationMiddleware(IConfiguration configuration)
        {
            _secretToken = configuration["TELEGRAM_BOT_SECRET"];
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            // To access the RequestData
            var req = await context.GetHttpRequestDataAsync();

            if (!IsValidRequest(req))
            {
                var res = req!.CreateResponse(HttpStatusCode.Forbidden);
                await res.WriteStringAsync("Secret key is invalid");
                context.GetInvocationResult().Value = res;
                return;
            }

            await next(context);

            bool IsValidRequest(HttpRequestData httpRequestData)
            {
                var isSecretTokenProvided = httpRequestData.Headers.TryGetValues("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader);
                if (!isSecretTokenProvided) return false;
                return string.Equals(secretTokenHeader.First(), _secretToken, StringComparison.Ordinal);
            }
        }
    }
}
