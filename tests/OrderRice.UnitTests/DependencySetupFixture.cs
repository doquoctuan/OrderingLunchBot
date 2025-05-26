using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderLunch.ApiClients;
using OrderLunch.Helper;
using OrderLunch.Interfaces;
using OrderLunch.Persistence;
using OrderLunch.Services;
using Refit;
using Telegram.Bot;

namespace OrderLunch.UnitTests
{
    public class DependencySetupFixture
    {
        private IConfiguration _config;
        public IConfiguration Configuration
        {
            get
            {
                if (_config == null)
                {
                    var builder = new ConfigurationBuilder().AddJsonFile($"local.settings.json", optional: false);
                    _config = builder.Build();
                }

                return _config;
            }
        }
        public ServiceProvider ServiceProvider { get; private set; }

        public DependencySetupFixture()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(Configuration);

            serviceCollection.AddHttpClient("telegram_client")
                .AddTypedClient<ITelegramBotClient>(httpClient
                    => new TelegramBotClient(Configuration["TELEGRAM_BOT_TOKEN"], httpClient));

            serviceCollection.AddHttpClient("github_client", c =>
            {
                c.BaseAddress = new Uri("https://api.github.com");
                c.DefaultRequestHeaders.Add("Authorization", $"Token {Configuration["GITHUB_TOKEN"]}");
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
            serviceCollection
                 .AddRefitClient<IBinanceApiClient>()
                 .ConfigureHttpClient(c =>
                 {
                     c.BaseAddress = new Uri("https://binance43.p.rapidapi.com");
                     c.DefaultRequestHeaders.Add("x-rapidapi-key", Configuration["BinanceApiKey"]);
                 });

            serviceCollection
                 .AddRefitClient<IWhapiClient>()
                 .ConfigureHttpClient(c =>
                 {
                     c.BaseAddress = new Uri("https://gate.whapi.cloud");
                     c.DefaultRequestHeaders.Add("Authorization", $"Bearer {Configuration["WhapiApiKey"]}");
                 });

            serviceCollection.AddTransient<AuthTokenHandler>();
            serviceCollection.Decorate<IGoogleAuthService, CachedGoogleAuthService>();

            serviceCollection.AddSingleton<RedisHandler>();
            serviceCollection.AddSingleton<Constants>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
