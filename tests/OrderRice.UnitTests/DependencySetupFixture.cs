using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderRice.Extentions;
using OrderRice.Helper;
using OrderRice.Interfaces;
using OrderRice.Persistence;
using OrderRice.Services;
using Telegram.Bot;

namespace OrderRice.UnitTests
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
                c.BaseAddress = new Uri(Configuration["GITHUB_REPOSITORY_URL"]);
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

            serviceCollection.AddTransient<AuthTokenHandler>();
            serviceCollection.Decorate<IGoogleAuthService, CachedGoogleAuthService>();

            serviceCollection.AddSingleton<Constants>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
