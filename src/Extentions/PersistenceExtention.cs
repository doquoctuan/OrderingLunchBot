using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderRice.Interfaces;
using OrderRice.Persistence;
using OrderRice.Services;

namespace OrderRice.Extentions
{
    public static class PersistenceExtention
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            IConfiguration configuration = serviceProvider.GetService<IConfiguration>();
            string cosmosDbEndpoint = configuration["CosmosDBEndpoint"];
            string accountKey = configuration["CosmosDBAccountKey"];
            services.AddDbContext<OrderLunchDbContext>(options =>
            {
                options.UseCosmos(cosmosDbEndpoint, accountKey, databaseName: "OrderLunchDb");
            });

            //using var client = new CosmosClient(cosmosDbEndpoint, accountKey);
            //var db = client.CreateDatabaseIfNotExistsAsync("OrderLunchDb").GetAwaiter().GetResult();
            //var container = db.Database.CreateContainerIfNotExistsAsync("Users", "/username").GetAwaiter().GetResult();

            services.AddScoped<DbContext>(provider => provider.GetService<OrderLunchDbContext>());
            services.AddTransient<IUserService, UserService>();

            serviceProvider = services.BuildServiceProvider();
            OrderLunchDbContextSeed.SeedDataFromGoogleSheetAsync(serviceProvider.GetService<OrderLunchDbContext>());

            return services;
        }
    }
}
