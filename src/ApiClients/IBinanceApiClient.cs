using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderLunch.ApiClients
{
    /// <summary>
    /// This interface represents access to the Binance API.
    /// </summary>
    public interface IBinanceApiClient
    {
        [Get("/ticker/price?symbol={name}")]
        Task<TickerPriceItem> GetPriceBySymbol([AliasAs("name")] string symbolName = "BTCUSDT");
    }

    public record class TickerPriceItem
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("price")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal Price { get; set; }
    }

    public class DecimalJsonConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Parse the string as decimal
            return decimal.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // Write decimal as string
            writer.WriteStringValue(value.ToString());
        }
    }
}
