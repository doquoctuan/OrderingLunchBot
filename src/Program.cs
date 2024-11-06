using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderLunch;
using OrderLunch.Helper;
using OrderLunch.Interfaces;
using OrderLunch.Middlewares;
using OrderLunch.Persistence;
using OrderLunch.Services;
using Telegram.Bot;

var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.Process)
    ?? throw new ArgumentException("Can not get telegram token. Set TELEGRAM_BOT_TOKEN in environment setting");

var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.Process)
    ?? throw new ArgumentException("Can not get githubToken. Set GITHUB_TOKEN in environment setting");

var appConfigConnectionStr = Environment.GetEnvironmentVariable("AppConfiguration", EnvironmentVariableTarget.Process);

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
        } else
        {
            config.AddAzureAppConfiguration(options =>
            {
                options.Connect(appConfigConnectionStr);

                // Configure Key Vault access
                options.ConfigureKeyVault(keyVaultOptions =>
                {
                    keyVaultOptions.SetCredential(new DefaultAzureCredential());
                });
            });
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
        serviceCollection.AddScoped(typeof(GoogleSheetContext));

        // Add business-logic service
        serviceCollection.AddScoped<IGoogleAuthService, GoogleAuthService>();
        serviceCollection.AddScoped<UpdateService>();
        serviceCollection.AddScoped<GithubService>();
        serviceCollection.AddScoped<IOrderService, OrderService>();

        serviceCollection.AddTransient<AuthTokenHandler>();
        serviceCollection.Decorate<IGoogleAuthService, CachedGoogleAuthService>();

        serviceCollection.AddSingleton<RedisHandler>();
        serviceCollection.AddSingleton<Constants>();

    })
    .Build();

host.Run();
