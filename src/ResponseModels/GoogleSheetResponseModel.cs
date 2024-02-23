using System.Text.Json.Serialization;

namespace OrderRice.ResponseModels
{
    public class GoogleSheetResponseModel
    {
        [JsonPropertyName("properties")]
        public DetailGoogleSheetResponse Properties { get; set; }
    }

    public class DetailGoogleSheetResponse
    {
        [JsonPropertyName("sheetId")]
        public string SheetId { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("index")]
        public string Index { get; set; }
        [JsonPropertyName("sheetType")]
        public string SheetType { get; set; }
        [JsonPropertyName("hidden")]
        public string Hidden { get; set; }
    }
}
