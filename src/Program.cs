using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderRice;
using OrderRice.Interfaces;
using OrderRice.Services;
using Telegram.Bot;

var tgToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.Process)
    ?? throw new ArgumentException("Can not get token. Set token in environment setting");

var repositoryUrl = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_URL", EnvironmentVariableTarget.Process)
    ?? throw new ArgumentException("Can not get repository url. Set repository url in environment setting");

var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.Process)
    ?? throw new ArgumentException("Can not get githubToken. Set githubToken in environment setting");

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(serviceCollection =>
    {
        serviceCollection.AddHttpClient("telegram_client")
            .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(tgToken, httpClient));

        serviceCollection.AddHttpClient("github_client", c =>
        {
            c.BaseAddress = new Uri(repositoryUrl);
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

        // Add business-logic service
        serviceCollection.AddScoped<IGoogleAuthService, GoogleAuthService>();
        serviceCollection.AddScoped<TelegramService>();
        serviceCollection.AddScoped<GithubService>();
        serviceCollection.AddScoped<IOrderService, OrderService>();

        serviceCollection.AddTransient<AuthTokenHandler>();
        serviceCollection.Decorate<IGoogleAuthService, CachedGoogleAuthService>();

        serviceCollection.AddSingleton<Constants>();

    })
    .Build();

host.Run();
