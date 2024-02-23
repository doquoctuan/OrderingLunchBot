using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OrderRice.Interfaces;
using OrderRice.ResponseModels;
using System.Text;

namespace OrderRice.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpGoogleClient;
        private readonly string spreadSheetId;

        public OrderService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpGoogleClient = httpClientFactory.CreateClient("google_sheet_client");
            spreadSheetId = configuration["SpreadSheetId"];
        }

        private async Task<string> FindSheetIdCurrentMonth()
        {
            var response = await _httpGoogleClient.GetAsync($"/v4/spreadsheets/{spreadSheetId}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            dynamic jsonConvert = JsonConvert.DeserializeObject(body);
            string sheetStr = jsonConvert.sheets;
            var sheets = JsonConvert.DeserializeObject<List<GoogleSheetResponseModel>>(sheetStr);
            return sheets.First().Properties.SheetId;
        }

        private async Task<List<List<string>>> GetSpreadSheetData(string sheetId)
        {
            var payload = new
            {
                dataFilters = new[] { new { gridRange = new { sheetId, startColumnIndex = 1 } } },
                majorDimension = "COLUMNS"
            };
            var json = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpGoogleClient.PostAsync($"/v4/spreadsheets/{spreadSheetId}/values:batchGetByDataFilter", httpContent);
            var body = await response.Content.ReadAsStringAsync();
            dynamic jsonConvert = JsonConvert.DeserializeObject(body);
            var listResult = jsonConvert.valueRanges[0].valueRange.values;
            return JsonConvert.DeserializeObject<List<List<string>>>(Convert.ToString(listResult));
        }

        public async Task<(string, string)> CreateOrderListImage()
        {
            var sheetId = await FindSheetIdCurrentMonth();
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new Exception("Cannot find sheetId for the current month");
            }
            await GetSpreadSheetData(sheetId);
            string urlImage = string.Empty;
            string nameImage = string.Empty;
            return new(urlImage, nameImage);
        }

        public Task<Dictionary<string, string>> GetMenu(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Order(string userName, DateTime dateTime, bool isOrder = true)
        {
            throw new NotImplementedException();
        }
    }
}
