using System.Text.Json.Serialization;

namespace SubscriptionBillingService.Infrastructure.Paymob.Models;

public class PaymobRefundRequest
{
    [JsonPropertyName("transaction_id")]
    public long TransactionId { get; set; }

    [JsonPropertyName("amount_cents")]
    public int AmountCents { get; set; }
}

public class PaymobRefundResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("amount_cents")]
    public int AmountCents { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("is_auth")]
    public bool IsAuth { get; set; }

    [JsonPropertyName("is_capture")]
    public bool IsCapture { get; set; }

    [JsonPropertyName("is_standalone_payment")]
    public bool IsStandalonePayment { get; set; }

    [JsonPropertyName("is_voided")]
    public bool IsVoided { get; set; }

    [JsonPropertyName("is_refunded")]
    public bool IsRefunded { get; set; }

    [JsonPropertyName("is_refund")]
    public bool IsRefund { get; set; }

    [JsonPropertyName("parent_transaction")]
    public long? ParentTransaction { get; set; }
}
