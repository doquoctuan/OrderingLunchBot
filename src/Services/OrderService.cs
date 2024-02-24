using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderRice.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using System.Collections.Generic;
using System.Text;

namespace OrderRice.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpGoogleClient;
        private readonly GithubService _githubService;
        private readonly string spreadSheetId;
        private readonly string BASE_IMAGE_URL;
        private const int SIZE_LIST = 23;
        private const int FONT_SIZE = 40;

        public OrderService(IHttpClientFactory httpClientFactory, IConfiguration configuration, GithubService githubService)
        {
            _httpGoogleClient = httpClientFactory.CreateClient("google_sheet_client");
            _githubService = githubService;
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
            var currentDate = dateTime.ToString("dd/MM/yyyy");
            int currentDateItem = 0;
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

        private bool IsUnPaid(List<List<string>> datas, int index)
        {
            return !datas[1][index].Equals("v", StringComparison.OrdinalIgnoreCase) && !datas[2][index].Equals("0");
        }

        private async Task<List<(string, string)>> ProcessCreateImage(List<List<string>> datas, int indexCurrentDate, Image<Rgba32> baseImage)
        {
            var prevSheetId = await FindSheetId(DateTime.Now.AddMonths(-1));
            var prevSpreadSheetData = await GetSpreadSheetData(prevSheetId);

            HashSet<int> debtSet = new();
            Dictionary<string, string> registerLunchTodays = new();

            // Get index of the debt list
            for (int i = 0; i < prevSpreadSheetData[1].Count; i++)
            {
                if (IsUnPaid(prevSpreadSheetData, i))
                {
                    debtSet.Add(i);
                }
            }

            // Get the list registration lunch today
            for (int i = 0; i < datas[indexCurrentDate].Count; i++)
            {
                if (datas[indexCurrentDate][i].Equals("x", StringComparison.OrdinalIgnoreCase))
                {
                    registerLunchTodays.Add(datas[0][i], debtSet.Contains(i) ? "Nợ" : "");
                }
            }

            List<(string, string)> result = new();

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
                    image.Mutate(ctx => ctx.DrawText(l.Key, font, color, location: new PointF(434, start + step)));
                    step += 59;
                    index++;
                }

                var response = await _githubService
                                        .UploadImageAsync(image.ToBase64String(PngFormat.Instance)
                                        .Split(';')[1]
                                        .Replace("base64,", ""), "list");
                result.Add(new(response.Content.DownloadUrl, $"Ảnh {countImage++}"));
            }

            return result;
        }

        public async Task<List<(string, string)>> CreateOrderListImage()
        {
            var sheetId = await FindSheetId(DateTime.Now);
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new Exception("Cannot find sheetId for the current month");
            }
            var spreadSheetData = await GetSpreadSheetData(sheetId);
            

            var indexCurrentDate = GetIndexCurrentDate(spreadSheetData, DateTime.Now);
            if (indexCurrentDate == 0)
            {
                throw new Exception("Today, do not support registration for lunch");
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
    }
}
