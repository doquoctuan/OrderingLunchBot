using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Logging;
using OrderLunch.Entities;
using OrderLunch.GoogleSheetModels;
using OrderLunch.Persistence;

namespace OrderLunch.Extentions
{
    public static class OrderLunchDbContextSeed
    {
        const string SPREADSHEET_ID = "1gPtS_06i20E-wBKzhQKeNsVdU6ITQtXDNjDsTuJ7n_g";
        const string SHEET_NAME = "LIST";

        public static void SeedDataFromGoogleSheetAsync(OrderLunchDbContext context, SpreadsheetsResource.ValuesResource _googleSheetValues)
        {
            Logger("Starting Migrate Database");
            context.Database.EnsureCreated();

            var range = $"{SHEET_NAME}!A2:J";
            var request = _googleSheetValues.Get(SPREADSHEET_ID, range);
            var response = request.Execute();
            var values = response.Values;

            var mapperValue = UsersMapper.MapFromRangeData(values);

            var users = context.Users.ToList();

            bool isAdd;

            foreach (var value in mapperValue)
            {
                isAdd = false;
                User userModel = users.Find(x => x.UserName == value.UserName);
                if (userModel is null)
                {
                    userModel = new User
                    {
                        UserName = value.UserName,
                        Department = value.Department
                    };

                    isAdd = true;
                }
                userModel.Birthday = value.Birthday;
                userModel.DayIn = value.DayIn;
                userModel.Email = value.Email;
                userModel.FullName = value.FullName;
                userModel.IsBlacklist = value.IsBlacklist;
                userModel.PhoneNumber = value.PhoneNumber;

                if (isAdd)
                {
                    context.Users.Add(userModel);
                }
            }

            context.Users.UpdateRange(users);
            context.SaveChanges();

            Logger("Migrate Database Successful");
        }

        private static void Logger(string message)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger(string.Empty);
            logger.LogInformation(message);
        }
    }
}
