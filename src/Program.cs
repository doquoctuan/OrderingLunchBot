using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderLunch;
using OrderLunch.ApiClients;
using OrderLunch.Helper;
using OrderLunch.Interfaces;
using OrderLunch.Middlewares;
using OrderLunch.Persistence;
using OrderLunch.Services;
using Refit;
using Telegram.Bot;
using UTC2_Tool.Context;

var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.Process)
    ?? throw new ArgumentException("Can not get telegram token. Set TELEGRAM_BOT_TOKEN in environment setting");

var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.Process)
    ?? throw new ArgumentException("Can not get githubToken. Set GITHUB_TOKEN in environment setting");

var binanceKey = Environment.GetEnvironmentVariable("BinanceApiKey", EnvironmentVariableTarget.Process);

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(workerApplication =>
    {
        workerApplication.UseWhen<TelegramAuthenticationMiddleware>((context) =>
        {
            // We want to use this middleware only for http trigger invocations.
            return context.FunctionDefinition.Name.Equals(Constants.TELEGRAM_HOOK_NAME)
                && context.FunctionDefinition.InputBindings.Values
                          .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";
        });
    })
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
    })
    .ConfigureServices(serviceCollection =>
    {
        serviceCollection.AddHttpClient("telegram_client")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(telegramToken, httpClient));

        serviceCollection.AddHttpClient("github_client", c =>
        {
            c.BaseAddress = new Uri("https://api.github.com");
            c.DefaultRequestHeaders.Add("Authorization", $"Token {githubToken}");
            c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            c.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.36.3");
        });

        serviceCollection.AddHttpClient<IGoogleAuthService>("google_auth_client", c =>
        {
            c.BaseAddress = new Uri("https://accounts.google.com");
        });

        serviceCollection.AddHttpClient("google_sheet_client", c =>
        {
            c.BaseAddress = new Uri("https://sheets.googleapis.com");
        }).AddHttpMessageHandler<AuthTokenHandler>();

        // Add Persistence
        serviceCollection.AddSingleton(typeof(GoogleSheetsHelper));
        serviceCollection.AddSingleton<DapperContext>();
        serviceCollection.AddScoped(typeof(GoogleSheetContext));

        // Add business-logic service
        serviceCollection.AddScoped<IGoogleAuthService, GoogleAuthService>();
        serviceCollection.AddScoped<UpdateService>();
        serviceCollection.AddScoped<GithubService>();
        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<IOrderService, OrderService>();
        serviceCollection.AddScoped<IPaymentService, PaymentService>();
        serviceCollection
            .AddRefitClient<IBinanceApiClient>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://binance43.p.rapidapi.com");
                c.DefaultRequestHeaders.Add("x-rapidapi-key", binanceKey);
            });

        serviceCollection.AddTransient<AuthTokenHandler>();
        serviceCollection.Decorate<IGoogleAuthService, CachedGoogleAuthService>();

        serviceCollection.AddSingleton<RedisHandler>();
        serviceCollection.AddSingleton<Constants>();

    })
    .Build();

host.Run();
