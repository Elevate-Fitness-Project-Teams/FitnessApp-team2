using System.Text.Json.Serialization;

namespace SubscriptionBillingService.Infrastructure.Paymob.Models;

public class PaymobIntentionRequest
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "EGP";

    [JsonPropertyName("payment_methods")]
    public List<int> PaymentMethods { get; set; } = new();

    [JsonPropertyName("items")]
    public List<PaymobIntentionItem> Items { get; set; } = new();

    [JsonPropertyName("billing_data")]
    public PaymobBillingData BillingData { get; set; } = new();

    [JsonPropertyName("special_reference")]
    public string SpecialReference { get; set; } = string.Empty;

    [JsonPropertyName("notification_url")]
    public string? NotificationUrl { get; set; }

    [JsonPropertyName("redirection_url")]
    public string? RedirectionUrl { get; set; }

    [JsonPropertyName("extras")]
    public Dictionary<string, string> Extras { get; set; } = new();
}

public class PaymobIntentionItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

public class PaymobBillingData
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = "Elevate";

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = "Member";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "user@elevatefitness.com";

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = "+201000000000";

    [JsonPropertyName("country")]
    public string Country { get; set; } = "EG";
}
