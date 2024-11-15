using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderLunch.Interfaces;
using OrderLunch.Persistence;
using OrderLunch.Exceptions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text;
using Color = SixLabors.ImageSharp.Color;
using OrderLunch.ApiClients;

namespace OrderLunch.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpGoogleClient;
        private readonly GithubService _githubService;
        private readonly GoogleSheetContext _googleSheetContext;
        private readonly ILogger<OrderService> _logger;
        private readonly IBinanceApiClient _binanceApiClient;
        private readonly string spreadSheetId;
        private readonly string centralSpreadSheetId;
        private readonly string BASE_IMAGE_URL;
        private readonly string BASE_IMAGE_UNPAID_URL;
        private const int SIZE_LIST = 23;
        private const int FONT_SIZE = 40;

        public OrderService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            GithubService githubService,
            GoogleSheetContext googleSheetContext,
            ILogger<OrderService> logger,
            IBinanceApiClient binanceApiClient)
        {
            _httpGoogleClient = httpClientFactory.CreateClient("google_sheet_client");
            _githubService = githubService;
            _logger = logger;
            spreadSheetId = configuration["SpreadSheetId"];
            centralSpreadSheetId = configuration["CentralSpreadSheetId"];
            BASE_IMAGE_URL = configuration["BASE_IMAGE"];
            BASE_IMAGE_UNPAID_URL = configuration["BASE_IMAGE_UNPAID"];
            _googleSheetContext = googleSheetContext;
            _binanceApiClient = binanceApiClient;
        }

        private async Task<string> FindSheetId(DateTime dateTime, string spearchSheet = null)
        {
            string searchPattern = $"T{dateTime:M.yyyy}";
            if (spearchSheet is null)
            {
                spearchSheet = spreadSheetId;
                searchPattern = $"T{dateTime:M/yyyy}";
            }
            var response = await _httpGoogleClient.GetAsync($"/v4/spreadsheets/{spearchSheet}");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var jObject = JsonConvert.DeserializeObject<JObject>(body);
            var jArraySheets = (JArray)jObject["sheets"];
            foreach (var sheet in jArraySheets)
            {
                var title = sheet.SelectToken("properties.title").ToString();
                if (title.Equals(searchPattern))
                {
                    return sheet.SelectToken("properties.sheetId").ToString();
                }
            }
            return string.Empty;
        }

        private async Task<List<List<string>>> GetSpreadSheetData(string sheetId, string spearchSheet = null)
        {
            spearchSheet ??= spreadSheetId;
            var payload = new
            {
                dataFilters = new[] { new { gridRange = new { sheetId, startColumnIndex = 1 } } },
                majorDimension = "COLUMNS"
            };
            var json = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpGoogleClient.PostAsync($"/v4/spreadsheets/{spearchSheet}/values:batchGetByDataFilter", httpContent);
            var body = await response.Content.ReadAsStringAsync();
            dynamic jsonConvert = JsonConvert.DeserializeObject(body);
            var listResult = jsonConvert.valueRanges[0].valueRange.values;
            return JsonConvert.DeserializeObject<List<List<string>>>(Convert.ToString(listResult));
        }

        private async Task<bool> WriteSpreadSheet(DateTime startDate, int startColumnIndex, int endColumnIndex, int rowIndex, string sheetId, object val = default, bool isAllowWeeken = false, string spearchSheet = null)
        {
            spearchSheet ??= spreadSheetId;
            object[][] values = new object[1][];
            int totalItem = endColumnIndex - startColumnIndex;
            for (int i = 0; i < totalItem; i++)
            {
                if (i == 0)
                {
                    values[i] = new object[totalItem];
                }
                var isWeeken = startDate.DayOfWeek == DayOfWeek.Sunday || startDate.DayOfWeek == DayOfWeek.Saturday;
                values[0][i] = !isWeeken || isAllowWeeken ? val : string.Empty;
                startDate = startDate.AddDays(1);
            }

            var payload = new
            {
                valueInputOption = "RAW",
                data = new[]
                {
                    new
                    {
                        dataFilter = new {
                            gridRange = new {
                                    sheetId,
                                    startColumnIndex,
                                    endColumnIndex,
                                    startRowIndex = rowIndex,
                                    endRowIndex = rowIndex + totalItem,
                            }
                        },
                        values
                    }
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpGoogleClient.PostAsync($"/v4/spreadsheets/{spearchSheet}/values:batchUpdateByDataFilter", httpContent);
            return response.IsSuccessStatusCode;
        }

        private int GetIndexDateColumn(List<List<string>> array2D, DateTime dateTime)
        {
            int currentDateItem = 0;
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

        private async Task<(List<(string, string)>, string, string)> ProcessCreateImage(List<List<string>> datas, int indexCurrentDate, Image<Rgba32> baseImage, string folderName = "list")
        {
            var blackListUsers = _googleSheetContext.Users.Where(x => x.IsBlacklist == true).Select(x => x.FullName).ToList();

            bool IsContain(List<string> blackListUsers, string name)
            {
                foreach (var fullName in blackListUsers)
                {
                    if (name.Contains(fullName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }

            void AddList(List<string> floor16, List<string> floor19, string name)
            {
                if (IsContain(blackListUsers, name) || name.Contains("OS") || name.Contains("TTS"))
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

            HashSet<string> deptSet = [];
            List<string> floor16 = [];
            List<string> floor19 = [];

            try
            {
                var deptMap = await UnPaidList();
                deptSet = deptMap.Select(x => x.Value.Item1).ToHashSet();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {Message}", ex.Message);
            }

            Dictionary<string, string> registerLunchTodays = [];
            string statusPaid;
            // Get the list registration lunch today
            for (int i = 0; i < datas[indexCurrentDate].Count; i++)
            {
                statusPaid = "";
                if (datas[indexCurrentDate][i].Trim().Equals("x", StringComparison.OrdinalIgnoreCase))
                {
                    var name = datas[0][i];
                    AddList(floor16, floor19, name);
                    if (deptSet.Contains(name))
                    {
                        statusPaid = "Nợ";
                        AddList(floor16, floor19, name);
                        AddList(floor16, floor19, name);
                    }
                    registerLunchTodays.Add(name, statusPaid);
                }
            }

            List<(string, string)> listRegister = [];
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
                                        .Replace("base64,", ""), folderName, prefixName: folderName);
                listRegister.Add(new(response.Content.DownloadUrl, $"Ảnh {countImage++}"));
            }

            // Random user pick ticket for lunch
            Random random = new();
            int randomIndexFloor19 = random.Next(0, floor19.Count);

            return new(listRegister, floor16.Count > 0 ? await GenerateMessageTakeTicket(floor16) : string.Empty, floor19.Count > 0 ? floor19[randomIndexFloor19] : string.Empty);
        }

        public async Task<string> GenerateMessageTakeTicket(List<string> users)
        {
            static int ExtractRandomIndexFromPrice(decimal price, int userCount)
            {
                string[] parts = price.ToString("F").Split('.');
                string combinedPart = string.Concat(parts[0], parts[1]);

                int hash = combinedPart.GetHashCode();
                return Math.Abs(hash) % userCount;
            }

            StringBuilder message = new("Giá Bitcoin hiện tại là: ");
            var symbolPrice = await _binanceApiClient.GetPriceBySymbol();
            message.Append(symbolPrice.Price.ToString("F"));
            message.Append(" USDT");
            int randomIndex = ExtractRandomIndexFromPrice(symbolPrice.Price, users.Count);
            message.AppendLine($"\nKính mời đồng chí {users[randomIndex >= 0 ? randomIndex : 0]} ");
            message.Append("lấy phiếu ăn ngày hôm nay.");
            return message.ToString();
        }

        public async Task<(List<(string, string)>, string, string)> CreateOrderListImage(string folderName)
        {
            var sheetId = await FindSheetId(DateTime.Now);
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new OrderServiceException(ErrorMessages.CANNOT_FIND_SHEET);
            }
            var spreadSheetData = await GetSpreadSheetData(sheetId);

            var indexCurrentDate = GetIndexDateColumn(spreadSheetData, DateTime.Now);
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

                return await ProcessCreateImage(spreadSheetData, indexCurrentDate, baseImage, folderName: folderName);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetMenu(DateTime dateTime)
        {
            var sheetId = await FindSheetId(DateTime.Now);
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new OrderServiceException(ErrorMessages.CANNOT_FIND_SHEET);
            }
            var spreadSheetData = await GetSpreadSheetData(sheetId);

            string dateTimeToString = dateTime.ToString("dd/MM");
            string dateTimeToStringWithAnother = dateTime.ToString("d/M");
            Dictionary<string, string> menu = new();
            foreach (var item in spreadSheetData)
            {
                if (item.Any() && (bool)(item?[0].Contains("Thực đơn", StringComparison.OrdinalIgnoreCase)))
                {
                    for (int i = 1; i < item.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(item[i])
                            && (item[i].Contains(dateTimeToString)
                            || item[i].Contains(dateTimeToStringWithAnother)))
                        {
                            int j = i + 1;
                            while (j < item.Count && !string.IsNullOrEmpty(item[j]))
                            {
                                menu.Add(item[j++], string.Empty);
                            }
                            break;
                        }
                    }
                    break;
                }
            }
            return menu;
        }

        private DateTime GetLastDayOfMonth(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }

        public async Task<bool> Order(string userName, DateTime dateTime, bool isOrder = true, bool isAll = false)
        {
            var user = _googleSheetContext.Users.Where(x => x.UserName == userName).FirstOrDefault();
            if (user is null)
            {
                throw new OrderServiceException(ErrorMessages.USER_DOES_NOT_EXIST);
            }

            var sheetId = await FindSheetId(DateTime.Now);
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new OrderServiceException(ErrorMessages.CANNOT_FIND_SHEET);
            }
            var spreadSheetData = await GetSpreadSheetData(sheetId);
            var indexColumnStart = GetIndexDateColumn(spreadSheetData, dateTime) + 1;
            var indexColumnEnd = indexColumnStart + 1;

            if (isAll)
            {
                indexColumnEnd = GetIndexDateColumn(spreadSheetData, GetLastDayOfMonth(dateTime)) + 2;
            }

            int userRow = 0;
            for (int i = 0; i < spreadSheetData[0].Count; i++)
            {
                if (spreadSheetData[0][i].Contains(user.FullName.Trim()))
                {
                    userRow = i;
                    break;
                }
            }

            if (userRow == 0)
            {
                throw new OrderServiceException(ErrorMessages.USER_DOES_NOT_EXIST);
            }

            return await WriteSpreadSheet(dateTime, indexColumnStart, indexColumnEnd, userRow, sheetId, isOrder ? "x" : string.Empty, isAllowWeeken: !isAll);
        }

        public async Task<List<(string, string)>> CreateUnpaidImage()
        {
            try
            {
                using HttpClient httpClient = new();
                byte[] baseImageBytes = await httpClient.GetByteArrayAsync(BASE_IMAGE_UNPAID_URL);
                using Image<Rgba32> baseImage = Image.Load<Rgba32>(baseImageBytes);

                // Draw date time
                baseImage.Mutate(ctx => ctx.DrawText(
                                            text: $"{DateTime.Now:dd/MM/yyyy}",
                                            font: new Font(SystemFonts.Get("Arial"), FONT_SIZE, FontStyle.Bold),
                                            color: new Color(Rgba32.ParseHex("#000000")),
                                            location: new PointF(895, 317)));

                var unpaidMap = await UnPaidList();

                List<(string, string)> listRegister = new();
                var registerLunchTodaysChunks = unpaidMap.Chunk(SIZE_LIST);
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
                        image.Mutate(ctx => ctx.DrawText($"{l.Value.Item2 * 30}K", font, color, location: new PointF(1100, start + step)));
                        image.Mutate(ctx => ctx.DrawText($"{index}", font, color, location: new PointF(index < 10 ? 252 : 240, start + step)));
                        image.Mutate(ctx => ctx.DrawText(l.Value.Item1, font, color, location: new PointF(390, start + step)));
                        step += 59;
                        index++;
                    }

                    listRegister.Add(new(image.ToBase64String(PngFormat.Instance)
                                            .Split(';')[1]
                                            .Replace("base64,", ""), $"Ảnh {countImage++}"));
                }

                return listRegister;

            }
            catch (Exception ex)
            {
                _logger.LogError(message: "An error occurred while creating an unpaid image - Stacktrace:\n {StackTrace}", ex.StackTrace);
                return [];
            }
        }

        public async Task<Dictionary<int, (string, int)>> UnPaidList()
        {
            var prevSheetId = await FindSheetId(DateTime.Now.AddMonths(-1));
            if (string.IsNullOrEmpty(prevSheetId))
            {
                throw new OrderServiceException(ErrorMessages.CANNOT_FIND_PREV_SHEET);
            }

            var prevSpreadSheetData = await GetSpreadSheetData(prevSheetId);

            Dictionary<int, (string, int)> debtMap = new();

            // Get index of the debt list
            for (int i = 0; i < prevSpreadSheetData[0].Count; i++)
            {
                (bool isUnpaid, string name, string totalTicket) = IsUnPaid(prevSpreadSheetData, i);
                if (isUnpaid)
                {
                    debtMap.Add(i, new(name, int.Parse(totalTicket)));
                }
            }

            return debtMap;

            static (bool, string, string) IsUnPaid(List<List<string>> datas, int index)
            {
                return new(string.IsNullOrEmpty(datas[1][index]) && !datas[2][index].Equals("0"), datas[0][index], datas[2][index]);
            }
        }

        public async Task BlockOrderTicket()
        {
            var sheetId = await FindSheetId(DateTime.Now);
            var spreadSheetData = await GetSpreadSheetData(sheetId);
            var indexColumnStart = GetIndexDateColumn(spreadSheetData, DateTime.Now) + 1;
            _googleSheetContext.ProtectedRange(spreadSheetId, int.Parse(sheetId), indexColumnStart);
        }

        public async Task<bool> PaymentConfirmation(string fullName)
        {
            var prevSheetId = await FindSheetId(DateTime.Now.AddMonths(-1));
            if (string.IsNullOrEmpty(prevSheetId))
            {
                throw new OrderServiceException(ErrorMessages.CANNOT_FIND_PREV_SHEET);
            }
            var spreadSheetData = await GetSpreadSheetData(prevSheetId);
            if (string.IsNullOrEmpty(fullName))
            {
                throw new Exception("This user could not be found");
            }
            for (int i = 0; i < spreadSheetData[0].Count; i++)
            {
                if (spreadSheetData[0][i].Contains(fullName, StringComparison.OrdinalIgnoreCase))
                {
                    int CHECKPAYMENT_COLUMN = 2;
                    return await WriteSpreadSheet(DateTime.Now, CHECKPAYMENT_COLUMN, CHECKPAYMENT_COLUMN + 1, i, prevSheetId, val: "x", isAllowWeeken: true);
                }
            }
            return false;
        }

        public async Task<(bool, int)> OrderTicket()
        {
            var currentDate = DateTime.Now;
            var devSheetId = await FindSheetId(currentDate);
            if (string.IsNullOrEmpty(devSheetId))
            {
                return (false, 0);
            }
            var devSpreadSheetData = await GetSpreadSheetData(devSheetId);
            var devIndexCurrentDate = GetIndexDateColumn(devSpreadSheetData, currentDate);
            if (devIndexCurrentDate == 0)
            {
                return (false, 0);
            }

            int totalTicket = 0;
            for (int i = 0; i < devSpreadSheetData[devIndexCurrentDate].Count; i++)
            {
                if (devSpreadSheetData[devIndexCurrentDate][i].Trim().Equals("x", StringComparison.OrdinalIgnoreCase))
                {
                    totalTicket++;
                }
            }

            if (totalTicket == 0)
            {
                return (false, 0);
            }

            var sheetId = await FindSheetId(dateTime: currentDate, spearchSheet: centralSpreadSheetId);
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new OrderServiceException("Không tìm thấy sheet đặt cơm của Trung tâm");
            }
            var spreadSheetData = await GetSpreadSheetData(sheetId, spearchSheet: centralSpreadSheetId);
            int indexDevDepartment = -1;
            for (int i = 0; i < spreadSheetData[0].Count; i++)
            {
                if (spreadSheetData[0][i].Equals("phòng phát triển", StringComparison.OrdinalIgnoreCase))
                {
                    indexDevDepartment = i;
                    break;
                }
            }
            if (indexDevDepartment == -1)
            {
                throw new Exception("Không tìm thấy sheet đặt cơm cho phòng Phát triển");
            }
            int indexCurrentDate = -1;
            for (int i = 0; i < spreadSheetData.Count; i++)
            {
                if (spreadSheetData[i][0].StartsWith($"{currentDate:dd/MM}"))
                {
                    indexCurrentDate = i;
                    break;
                }
            }
            if (indexCurrentDate == -1)
            {
                throw new Exception("Hôm nay Trung tâm không hỗ trợ đặt cơm");
            }
            indexCurrentDate++;
            return (await WriteSpreadSheet(currentDate, indexCurrentDate, indexCurrentDate + 1, indexDevDepartment, sheetId, val: totalTicket, isAllowWeeken: true, spearchSheet: centralSpreadSheetId), totalTicket);
        }
    }
}
