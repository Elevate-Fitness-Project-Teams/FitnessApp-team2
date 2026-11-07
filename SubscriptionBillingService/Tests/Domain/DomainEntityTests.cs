using SubscriptionBillingService.Common;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using Xunit;

namespace SubscriptionBillingService.Tests.Domain;

public class DomainEntityTests
{
    [Fact]
    public void UserSubscription_InitialState_DefaultsToFreeAndActive()
    {
        var sub = new UserSubscription
        {
            UserId = Guid.NewGuid()
        };

        Assert.Equal(SubscriptionTier.Free, sub.Tier);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        Assert.True(sub.AutoRenew);
        Assert.False(sub.IsDeleted);
        Assert.NotEqual(Guid.Empty, sub.Id);
    }

    [Fact]
    public void PaymentAttempt_InitialState_DefaultsToInitiated()
    {
        var attempt = new PaymentAttempt
        {
            UserId = Guid.NewGuid(),
            IdempotencyKey = "key-123",
            AmountCents = 19900
        };

        Assert.Equal(PaymentAttemptStatus.Initiated, attempt.Status);
        Assert.Equal("EGP", attempt.Currency);
        Assert.Equal(19900, attempt.AmountCents);
        Assert.True(attempt.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public void RefundRequest_InitialState_DefaultsToPending()
    {
        var refund = new RefundRequest
        {
            UserId = Guid.NewGuid(),
            PaymentAttemptId = Guid.NewGuid(),
            ProviderTransactionId = "tx-123456",
            AmountCents = 19900,
            Reason = "DuplicateActiveSubscription",
            RefundType = RefundType.AutoDuplicateRefund
        };

        Assert.Equal(RefundRequestStatus.Pending, refund.Status);
        Assert.Equal(RefundType.AutoDuplicateRefund, refund.RefundType);
        Assert.Equal(0, refund.RetryCount);
    }

    [Fact]
    public void Result_Success_ContainsValue()
    {
        var result = Result.Success("premium");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("premium", result.Value);
    }

    [Fact]
    public void Result_Failure_ContainsError()
    {
        var error = Error.Conflict("CONFLICT_CODE", "Conflict details");
        var result = Result<string>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }
}
