using System.Text.Json.Serialization;

namespace OrderLunch.Models;

public record PaymentWebhookDTO
{
    [JsonPropertyName("token")]
    public string Token { get; init; }

    [JsonPropertyName("payment")]
    public PaymentInfomation Payment { get; init; }
}

public record PaymentInfomation
{
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("content")]
    public string Content { get; init; }

    [JsonPropertyName("date")]
    public DateTime? CreatedAt { get; init; }

    [JsonPropertyName("account_receiver")]
    public string AccountNumber { get; init; }

    [JsonPropertyName("gate")]
    public string Gate { get; init; }
}
