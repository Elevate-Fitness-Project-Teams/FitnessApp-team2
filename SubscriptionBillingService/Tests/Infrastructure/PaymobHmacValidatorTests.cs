using SubscriptionBillingService.Infrastructure.Paymob;
using SubscriptionBillingService.Infrastructure.Paymob.Models;
using Xunit;

namespace SubscriptionBillingService.Tests.Infrastructure;

public class PaymobHmacValidatorTests
{
    private const string TestSecret = "d41d8cd98f00b204e9800998ecf8427e";

    [Fact]
    public void ValidateHmac_WhenDataAndSecretMatch_ReturnsTrue()
    {
        // Arrange
        var validator = new PaymobHmacValidator(TestSecret);
        var obj = new PaymobTransactionObject
        {
            AmountCents = 19900,
            CreatedAt = "2026-11-06T18:00:00.000Z",
            Currency = "EGP",
            ErrorOccured = false,
            HasParentTransaction = false,
            Id = 987654321,
            IntegrationId = 123456,
            Is3dSecure = true,
            IsAuth = false,
            IsCapture = false,
            IsRefunded = false,
            IsStandalonePayment = false,
            IsVoided = false,
            Order = new PaymobOrder { Id = 55443322 },
            Owner = 112233,
            Pending = false,
            SourceData = new PaymobSourceData
            {
                Pan = "2345",
                SubType = "MasterCard",
                Type = "card"
            },
            Success = true
        };

        var calculatedHmac = validator.ComputeHmac(obj);

        // Act
        var isValid = validator.ValidateHmac(obj, calculatedHmac);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateHmac_WhenAmountIsTampered_ReturnsFalse()
    {
        // Arrange
        var validator = new PaymobHmacValidator(TestSecret);
        var obj = new PaymobTransactionObject
        {
            AmountCents = 19900,
            Currency = "EGP",
            Id = 987654321,
            Success = true
        };

        var originalHmac = validator.ComputeHmac(obj);

        // Tamper with amount
        obj.AmountCents = 100;

        // Act
        var isValid = validator.ValidateHmac(obj, originalHmac);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateHmac_WhenReceivedHmacIsInvalidHex_ReturnsFalse()
    {
        var validator = new PaymobHmacValidator(TestSecret);
        var obj = new PaymobTransactionObject { Id = 123 };

        var isValid = validator.ValidateHmac(obj, "not_a_hex_string");

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateHmac_WhenReceivedHmacIsEmpty_ReturnsFalse()
    {
        var validator = new PaymobHmacValidator(TestSecret);
        var obj = new PaymobTransactionObject { Id = 123 };

        var isValid = validator.ValidateHmac(obj, "");

        Assert.False(isValid);
    }
}
