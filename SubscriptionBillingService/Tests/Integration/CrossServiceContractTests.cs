using System.Text.Json;
using SubscriptionBillingService.Contracts;
using Xunit;

namespace SubscriptionBillingService.Tests.Integration;

public class CrossServiceContractTests
{
    [Fact]
    public void SubscriptionUpgradedEvent_SerializationRoundTrip_PreservesAllFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var upgradedAt = DateTime.UtcNow;
        var expiresAt = upgradedAt.AddMonths(3);
        var original = new SubscriptionUpgradedEvent(
            userId,
            "Premium",
            3,
            upgradedAt,
            expiresAt,
            "tx_12345678");

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<SubscriptionUpgradedEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.UserId, deserialized.UserId);
        Assert.Equal(original.PlanTier, deserialized.PlanTier);
        Assert.Equal(original.DurationMonths, deserialized.DurationMonths);
        Assert.Equal(original.UpgradedAtUtc, deserialized.UpgradedAtUtc);
        Assert.Equal(original.ExpiresAtUtc, deserialized.ExpiresAtUtc);
        Assert.Equal(original.TransactionReference, deserialized.TransactionReference);
    }

    [Fact]
    public void SubscriptionExpiredEvent_SerializationRoundTrip_PreservesAllFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredAt = DateTime.UtcNow;
        var original = new SubscriptionExpiredEvent(userId, expiredAt);

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<SubscriptionExpiredEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.UserId, deserialized.UserId);
        Assert.Equal(original.ExpiredAtUtc, deserialized.ExpiredAtUtc);
    }

    [Fact]
    public void SubscriptionCancelledEvent_SerializationRoundTrip_PreservesAllFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancelledAt = DateTime.UtcNow;
        var effectiveExpiresAt = cancelledAt.AddMonths(1);
        var original = new SubscriptionCancelledEvent(userId, cancelledAt, effectiveExpiresAt);

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<SubscriptionCancelledEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.UserId, deserialized.UserId);
        Assert.Equal(original.CancelledAtUtc, deserialized.CancelledAtUtc);
        Assert.Equal(original.EffectiveExpiresAtUtc, deserialized.EffectiveExpiresAtUtc);
    }
}
