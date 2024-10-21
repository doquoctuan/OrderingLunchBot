using System.Text.Json.Serialization;

namespace OrderLunch.ResponseModels
{
    public class GitResponseModel
    {
        [JsonPropertyName("content")]
        public ContentResponse Content { get; set; }
    }

    public class ContentResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
