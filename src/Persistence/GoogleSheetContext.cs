using Microsoft.Extensions.Configuration;
using OrderRice.Entities;
using OrderRice.GoogleSheetModels;
using OrderRice.Helper;

namespace OrderRice.Persistence
{
    public class GoogleSheetContext
    {
        public GoogleSheetContext(GoogleSheetsHelper googleSheetsHelper, IConfiguration configuration)
        {
            string SPREADSHEET_ID = configuration["GoogleSheetDatasource"];
            string SHEET_NAME = configuration["GoogleSheetName"];
            var range = $"{SHEET_NAME}!A2:J";
            var request = googleSheetsHelper.Service.Spreadsheets.Values.Get(SPREADSHEET_ID, range);
            var response = request.Execute();
            var values = response.Values;
            Users = UsersMapper.MapFromRangeData(values);
        }

        public List<Users> Users { get; set; }
    }
}
