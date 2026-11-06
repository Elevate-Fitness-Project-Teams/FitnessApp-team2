using SubscriptionBillingService.Domain.Catalog;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using Xunit;

namespace SubscriptionBillingService.Tests.Domain;

public class SubscriptionPlanCatalogTests
{
    [Theory]
    [InlineData(SubscriptionTier.Premium, 1, 19900, 199.00)]
    [InlineData(SubscriptionTier.Premium, 3, 49900, 499.00)]
    [InlineData(SubscriptionTier.Premium, 6, 89900, 899.00)]
    [InlineData(SubscriptionTier.Premium, 12, 149900, 1499.00)]
    public void TryGetPlan_WhenValidPlanRequested_ReturnsCorrectPricingAndDetails(
        SubscriptionTier tier,
        int durationMonths,
        int expectedAmountCents,
        decimal expectedAmount)
    {
        // Act
        var found = SubscriptionPlanCatalog.TryGetPlan(tier, durationMonths, out var plan);

        // Assert
        Assert.True(found);
        Assert.NotNull(plan);
        Assert.Equal(expectedAmountCents, plan.AmountCents);
        Assert.Equal(expectedAmount, plan.Amount);
        Assert.Equal("EGP", plan.Currency);
        Assert.False(string.IsNullOrWhiteSpace(plan.Name));
    }

    [Theory]
    [InlineData(SubscriptionTier.Free, 1)]
    [InlineData(SubscriptionTier.Premium, 2)]
    [InlineData(SubscriptionTier.Premium, 24)]
    public void TryGetPlan_WhenInvalidPlanRequested_ReturnsFalse(SubscriptionTier tier, int durationMonths)
    {
        // Act
        var found = SubscriptionPlanCatalog.TryGetPlan(tier, durationMonths, out var plan);

        // Assert
        Assert.False(found);
        Assert.Null(plan);
    }

    [Fact]
    public void GetPlanOrThrow_WhenInvalid_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            SubscriptionPlanCatalog.GetPlanOrThrow(SubscriptionTier.Free, 0));
    }
}
