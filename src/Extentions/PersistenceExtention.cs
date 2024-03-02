using Google.Apis.Sheets.v4;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderRice.Helper;
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
            string connectionString = configuration["ConnectionString"];

            services.AddDbContext<OrderLunchDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });

            services.AddScoped<DbContext>(provider => provider.GetService<OrderLunchDbContext>());
            services.AddTransient<IUserService, UserService>();

            serviceProvider = services.BuildServiceProvider();

            SpreadsheetsResource.ValuesResource googleSheetsHelper = serviceProvider.GetService<GoogleSheetsHelper>().Service.Spreadsheets.Values;
            OrderLunchDbContextSeed.SeedDataFromGoogleSheetAsync(serviceProvider.GetService<OrderLunchDbContext>(), googleSheetsHelper);

            return services;
        }
    }
}
