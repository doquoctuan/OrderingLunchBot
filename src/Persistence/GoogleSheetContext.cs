using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using OrderLunch.Entities;
using OrderLunch.GoogleSheetModels;
using OrderLunch.Helper;

namespace OrderLunch.Persistence
{
    public class GoogleSheetContext
    {
        private readonly GoogleSheetsHelper _googleSheetsHelper;
        public GoogleSheetContext(GoogleSheetsHelper googleSheetsHelper, IConfiguration configuration)
        {
            string SPREADSHEET_ID = configuration["GoogleSheetDatasource"];
            string SHEET_NAME = configuration["GoogleSheetName"];
            var range = $"{SHEET_NAME}!A2:J";
            var request = googleSheetsHelper.Service.Spreadsheets.Values.Get(SPREADSHEET_ID, range);
            var response = request.Execute();
            var values = response.Values;
            Users = UsersMapper.MapFromRangeData(values);
            _googleSheetsHelper = googleSheetsHelper;
        }

        public List<User> Users { get; set; }

        public void ProtectedRange(string spreadSheetId, int sheetId, int StartColumnIndex)
        {
            var protectedRange = new ProtectedRange
            {
                Range = new GridRange
                {
                    SheetId = sheetId,
                    StartColumnIndex = StartColumnIndex,
                    EndColumnIndex = StartColumnIndex + 1,
                    StartRowIndex = 1,
                    EndRowIndex = 500,
                },
                WarningOnly = false,
                Description = "Quá thời gian đặt/huỷ cơm",
                Editors = new Editors
                {
                    DomainUsersCanEdit = false,
                    Users = new List<string> {
                    "phongphattrientest@gmail.com",
                    "vts-telebot@vts-tele-bot.iam.gserviceaccount.com",
                    "phongphattriengpmn@gmail.com",
                    "tuandoquoc28@gmail.com",
                }
                },
            };

            // Create the request
            var request = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
            {
                new Request
                {
                    AddProtectedRange = new AddProtectedRangeRequest
                    {
                        ProtectedRange = protectedRange,
                    },
                },
            },
            };

            // Execute the request
            var response = _googleSheetsHelper.Service.Spreadsheets.BatchUpdate(request, spreadSheetId).Execute();
        }
    }
}
