using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderRice.Entities;
using OrderRice.Exceptions;
using OrderRice.Interfaces;
using OrderRice.Persistence;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderRice.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpGoogleClient;
        private readonly GithubService _githubService;
        private readonly ILogger<OrderService> _logger;
        private readonly string spreadSheetId;
        private readonly string BASE_IMAGE_URL;
        private const int SIZE_LIST = 23;
        private const int FONT_SIZE = 40;

        public OrderService(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            GithubService githubService,
            ILogger<OrderService> logger)
        {
            _httpGoogleClient = httpClientFactory.CreateClient("google_sheet_client");
            _githubService = githubService;
            _logger = logger;
            spreadSheetId = configuration["SpreadSheetId"];
            BASE_IMAGE_URL = configuration["BASE_IMAGE"];
        }

        private async Task<string> FindSheetId(DateTime dateTime)
        {
            var response = await _httpGoogleClient.GetAsync($"/v4/spreadsheets/{spreadSheetId}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<JObject>(body);
            var jArraySheets = (JArray)jObject["sheets"];
            foreach (var sheet in jArraySheets)
            {
                var title = sheet.SelectToken("properties.title").ToString();
                if (title.Equals($"T{dateTime:M/yyyy}"))
                {
                    return sheet.SelectToken("properties.sheetId").ToString();
                }
            }
            return string.Empty;
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

        private int GetIndexCurrentDate(List<List<string>> array2D, DateTime dateTime)
        {
            int currentDateItem = 0;
            if (dateTime.DayOfWeek is DayOfWeek.Saturday || dateTime.DayOfWeek is DayOfWeek.Sunday)
            {
                return currentDateItem;
            }
            var currentDate = dateTime.ToString("dd/MM/yyyy");
            for (int i = 0; i < array2D.Count; i++)
            {
                if (currentDate.Contains(array2D[i][0]))
                {
                    currentDateItem = i;
                    break;
                }
            }
            return currentDateItem;
        }

        private async Task<(List<(string, string)>, string, string)> ProcessCreateImage(List<List<string>> datas, int indexCurrentDate, Image<Rgba32> baseImage)
        {
            static void AddList(List<string> floor16, List<string> floor19, string name)
            {
                if (BlackLists.Set.Contains(name))
                {
                    return;
                }

                if (name.Contains("19"))
                {
                    floor19.Add(name);
                }
                else
                {
                    floor16.Add(name);
                }
            }

            Dictionary<int, string> deptMap = new();
            List<string> floor16 = new();
            List<string> floor19 = new();
            try
            {
                deptMap = await UnPaidList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {Message}", ex.Message);
            }

            Dictionary<string, string> registerLunchTodays = new();
            string statusPaid;
            // Get the list registration lunch today
            for (int i = 0; i < datas[indexCurrentDate].Count; i++)
            {
                statusPaid = "";
                if (datas[indexCurrentDate][i].Equals("x", StringComparison.OrdinalIgnoreCase))
                {
                    var name = datas[0][i];
                    AddList(floor16, floor19, name);
                    if (deptMap.ContainsKey(i))
                    {
                        statusPaid = "Nợ";
                        AddList(floor16, floor19, name);
                        AddList(floor16, floor19, name);
                    }
                    registerLunchTodays.Add(name, statusPaid);
                }
            }

            List<(string, string)> listRegister = new();
            var registerLunchTodaysChunks = registerLunchTodays.Chunk(SIZE_LIST);
            int countImage = 1;
            var font = new Font(SystemFonts.Get("Arial"), FONT_SIZE, FontStyle.Regular);
            var color = new Color(Rgba32.ParseHex("#000000"));
            int index = 1;
            foreach (var list in registerLunchTodaysChunks)
            {
                int step = 0;
                int start = 535;
                var image = baseImage.Clone();
                foreach (var l in list)
                {
                    image.Mutate(ctx => ctx.DrawText(l.Value, font, color, location: new PointF(1140, start + step)));
                    image.Mutate(ctx => ctx.DrawText($"{index}", font, color, location: new PointF(index < 10 ? 252 : 240, start + step)));
                    image.Mutate(ctx => ctx.DrawText(l.Key, font, color, location: new PointF(390, start + step)));
                    step += 59;
                    index++;
                }

                var response = await _githubService
                                        .UploadImageAsync(image.ToBase64String(PngFormat.Instance)
                                        .Split(';')[1]
                                        .Replace("base64,", ""), "list");
                listRegister.Add(new(response.Content.DownloadUrl, $"Ảnh {countImage++}"));
            }

            // Random user pick ticket for lunch
            Random random = new();
            int randomIndexFloor16 = random.Next(0, floor16.Count);
            int randomIndexFloor19 = random.Next(0, floor19.Count);

            return new(listRegister, floor16.Count > 0 ? floor16[randomIndexFloor16] : string.Empty, floor19.Count > 0 ? floor19[randomIndexFloor19] : string.Empty);
        }

        public async Task<(List<(string, string)>, string, string)> CreateOrderListImage()
        {
            var sheetId = await FindSheetId(DateTime.Now);
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new OrderServiceException(ErrorMessages.CANNOT_FIND_SHEET);
            }
            var spreadSheetData = await GetSpreadSheetData(sheetId);
            
            var indexCurrentDate = GetIndexCurrentDate(spreadSheetData, DateTime.Now);
            if (indexCurrentDate == 0)
            {
                throw new OrderServiceException(ErrorMessages.CANNOT_FIND_SHEET_TODAY);
            }

            try
            {
                using HttpClient httpClient = new();
                byte[] baseImageBytes = await httpClient.GetByteArrayAsync(BASE_IMAGE_URL);
                using Image<Rgba32> baseImage = Image.Load<Rgba32>(baseImageBytes);

                // Draw date time
                baseImage.Mutate(ctx => ctx.DrawText(
                                            text: $"{DateTime.Now:dd/MM/yyyy}",
                                            font: new Font(SystemFonts.Get("Arial"), FONT_SIZE, FontStyle.Bold),
                                            color: new Color(Rgba32.ParseHex("#000000")),
                                            location: new PointF(895, 317)));

                return await ProcessCreateImage(spreadSheetData, indexCurrentDate, baseImage);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<Dictionary<string, string>> GetMenu(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Order(string userName, DateTime dateTime, bool isOrder = true)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<int, string>> UnPaidList()
        {
            var prevSheetId = await FindSheetId(DateTime.Now.AddMonths(-1));
            if (string.IsNullOrEmpty(prevSheetId))
            {
                throw new OrderServiceException(ErrorMessages.CANNOT_FIND_PREV_SHEET);
            }

            var prevSpreadSheetData = await GetSpreadSheetData(prevSheetId);

            Dictionary<int, string> debtMap = new();

            // Get index of the debt list
            for (int i = 0; i < prevSpreadSheetData[1].Count; i++)
            {
                (bool isUnpaid, string name) = IsUnPaid(prevSpreadSheetData, i);
                if (isUnpaid)
                {
                    debtMap.Add(i, name);
                }
            }

            return debtMap;

            static (bool, string) IsUnPaid(List<List<string>> datas, int index)
            {
                return new (!datas[1][index].Equals("v", StringComparison.OrdinalIgnoreCase) && !datas[2][index].Equals("0"), datas[0][index]);
            }
        }
    }
}
