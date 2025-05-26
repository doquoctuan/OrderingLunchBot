using Microsoft.AspNetCore.Mvc;
using Refit;
using System.Text.Json.Serialization;

namespace OrderLunch.ApiClients
{
    public interface IWhapiClient
    {
        [Post("/messages/text")]
        Task<ApiResponse<ResponseBodySendMessage>> SendMessages(MessagePayload messagePayload);

        [Post("/messages/image")]
        Task<ApiResponse<dynamic>> SendMediaMessages(SendMediaMsgPayload messagePayload);
    }

    public record class MessagePayload
    {
        [JsonPropertyName("body")]
        public string Body { get; init; }

        [JsonPropertyName("to")]
        public string To { get; init; } = "120363419971888833@g.us";
    }

    public record class SendMediaMsgPayload
    {
        [JsonPropertyName("caption")]
        public string Caption { get; init; }

        [JsonPropertyName("to")]
        public string To { get; init; } = "120363419971888833@g.us";

        [JsonPropertyName("media")]
        public string Media { get; init; }
    }

    public class ResponseBodySendMessage
    {
        public bool sent { get; set; }
        public Message message { get; set; }
    }

    public class Message
    {
        public string id { get; set; }
        public bool from_me { get; set; }
        public string type { get; set; }
        public string chat_id { get; set; }
        public int timestamp { get; set; }
        public string source { get; set; }
        public int device_id { get; set; }
        public string status { get; set; }
        public Text text { get; set; }
        public string from { get; set; }
    }

    public class Text
    {
        public string body { get; set; }
    }
}
