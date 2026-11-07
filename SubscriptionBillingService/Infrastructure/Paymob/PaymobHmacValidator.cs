using System.Security.Cryptography;
using System.Text;
using SubscriptionBillingService.Infrastructure.Paymob.Models;

namespace SubscriptionBillingService.Infrastructure.Paymob;

public class PaymobHmacValidator
{
    private readonly string _hmacSecret;

    public PaymobHmacValidator(string hmacSecret)
    {
        _hmacSecret = hmacSecret;
    }

    public bool ValidateHmac(PaymobTransactionObject obj, string? receivedHmac)
    {
        if (string.IsNullOrWhiteSpace(receivedHmac) || string.IsNullOrWhiteSpace(_hmacSecret))
        {
            return false;
        }

        var calculatedHmac = ComputeHmac(obj);

        var receivedBytes = ConvertHexToBytes(receivedHmac);
        var calculatedBytes = ConvertHexToBytes(calculatedHmac);

        if (receivedBytes == null || calculatedBytes == null || receivedBytes.Length != calculatedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(receivedBytes, calculatedBytes);
    }

    public string ComputeHmac(PaymobTransactionObject obj)
    {
        var sb = new StringBuilder();

        // 1. amount_cents
        sb.Append(obj.AmountCents);
        // 2. created_at
        sb.Append(obj.CreatedAt);
        // 3. currency
        sb.Append(obj.Currency);
        // 4. error_occured
        sb.Append(obj.ErrorOccured ? "true" : "false");
        // 5. has_parent_transaction
        sb.Append(obj.HasParentTransaction ? "true" : "false");
        // 6. id
        sb.Append(obj.Id);
        // 7. integration_id
        sb.Append(obj.IntegrationId);
        // 8. is_3d_secure
        sb.Append(obj.Is3dSecure ? "true" : "false");
        // 9. is_auth
        sb.Append(obj.IsAuth ? "true" : "false");
        // 10. is_capture
        sb.Append(obj.IsCapture ? "true" : "false");
        // 11. is_refunded
        sb.Append(obj.IsRefunded ? "true" : "false");
        // 12. is_standalone_payment
        sb.Append(obj.IsStandalonePayment ? "true" : "false");
        // 13. is_voided
        sb.Append(obj.IsVoided ? "true" : "false");
        // 14. order.id
        sb.Append(obj.Order?.Id.ToString() ?? "");
        // 15. owner
        sb.Append(obj.Owner);
        // 16. pending
        sb.Append(obj.Pending ? "true" : "false");
        // 17. source_data.pan
        sb.Append(obj.SourceData?.Pan ?? "");
        // 18. source_data.sub_type
        sb.Append(obj.SourceData?.SubType ?? "");
        // 19. source_data.type
        sb.Append(obj.SourceData?.Type ?? "");
        // 20. success
        sb.Append(obj.Success ? "true" : "false");

        var secretBytes = Encoding.UTF8.GetBytes(_hmacSecret);
        var dataBytes = Encoding.UTF8.GetBytes(sb.ToString());

        using var hmac = new HMACSHA512(secretBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static byte[]? ConvertHexToBytes(string hex)
    {
        try
        {
            return Convert.FromHexString(hex);
        }
        catch
        {
            return null;
        }
    }
}
