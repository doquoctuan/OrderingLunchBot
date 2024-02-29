using Google.Apis.Sheets.v4;
using OrderRice.GoogleSheetModels;
using OrderRice.Persistence;

namespace OrderRice.Extentions
{
    public static class OrderLunchDbContextSeed
    {
        const string SPREADSHEET_ID = "1gPtS_06i20E-wBKzhQKeNsVdU6ITQtXDNjDsTuJ7n_g";
        const string SHEET_NAME = "LIST";

        public static void SeedDataFromGoogleSheetAsync(OrderLunchDbContext context, SpreadsheetsResource.ValuesResource _googleSheetValues)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var range = $"{SHEET_NAME}!A2:H";
            var request = _googleSheetValues.Get(SPREADSHEET_ID, range);
            var response = request.Execute();
            var values = response.Values;

            var mapperValue = UsersMapper.MapFromRangeData(values);

            context.Users.AddRange(mapperValue);
            context.SaveChanges();

        }
    }
}
