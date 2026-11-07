using System.Text.Json.Serialization;

namespace SubscriptionBillingService.Infrastructure.Paymob.Models;

public class PaymobIntentionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("special_reference")]
    public string SpecialReference { get; set; } = string.Empty;
}
