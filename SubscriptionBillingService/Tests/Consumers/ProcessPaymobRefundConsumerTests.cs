using MassTransit;
using MassTransit.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Consumers;
using SubscriptionBillingService.Infrastructure.Paymob;
using SubscriptionBillingService.Infrastructure.Paymob.Models;
using Xunit;

namespace SubscriptionBillingService.Tests.Consumers;

public class ProcessPaymobRefundConsumerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SubscriptionDbContext _dbContext;

    public ProcessPaymobRefundConsumerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new SubscriptionDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    private class MockPaymobGatewayClient : IPaymobGatewayClient
    {
        public bool ShouldSucceed { get; set; } = true;
        public long RefundTxId { get; set; } = 99887766;

        public Task<PaymobIntentionResponse> CreateIntentionAsync(PaymobIntentionRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PaymobRefundResponse> RefundTransactionAsync(PaymobRefundRequest request, CancellationToken cancellationToken = default)
        {
            if (!ShouldSucceed)
            {
                throw new HttpRequestException("Paymob 503 Service Unavailable");
            }

            return Task.FromResult(new PaymobRefundResponse
            {
                Id = RefundTxId,
                Success = true,
                IsRefund = true,
                AmountCents = request.AmountCents,
                ParentTransaction = request.TransactionId
            });
        }

        public Task<PaymobTransactionObject> GetTransactionByIdAsync(long transactionId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task Consume_WhenRefundSucceeds_UpdatesDatabaseAtomicallyAndPublishesEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = new UserSubscription
        {
            UserId = userId,
            Tier = SubscriptionTier.Premium,
            Status = SubscriptionStatus.Active
        };
        _dbContext.UserSubscriptions.Add(subscription);

        var attempt = new PaymentAttempt
        {
            UserId = userId,
            IdempotencyKey = "dup-key",
            AmountCents = 19900,
            Currency = "EGP",
            Status = PaymentAttemptStatus.DuplicateChargePendingRefund,
            ProviderTransactionId = "987654321"
        };
        _dbContext.PaymentAttempts.Add(attempt);

        var refundRequest = new RefundRequest
        {
            UserId = userId,
            PaymentAttemptId = attempt.Id,
            ProviderTransactionId = "987654321",
            AmountCents = 19900,
            Currency = "EGP",
            Reason = "DuplicateActiveSubscription",
            RefundType = RefundType.AutoDuplicateRefund,
            Status = RefundRequestStatus.Pending
        };
        _dbContext.RefundRequests.Add(refundRequest);
        await _dbContext.SaveChangesAsync();

        var mockPaymob = new MockPaymobGatewayClient { ShouldSucceed = true, RefundTxId = 11223344 };

        var services = new ServiceCollection();
        services.AddSingleton(_dbContext);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddSingleton<IPaymobGatewayClient>(mockPaymob);
        services.AddSingleton<ILogger<ProcessPaymobRefundConsumer>>(NullLogger<ProcessPaymobRefundConsumer>.Instance);

        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<ProcessPaymobRefundConsumer>();
        });

        var provider = services.BuildServiceProvider();
        var harness = provider.GetRequiredService<ITestHarness>();
        harness.TestTimeout = TimeSpan.FromSeconds(10);

        await harness.Start();

        try
        {
            var command = new ExecutePaymobRefundCommandEvent(
                refundRequest.Id,
                userId,
                attempt.Id,
                "987654321",
                19900,
                "EGP",
                "DuplicateActiveSubscription");

            // Act
            await harness.Bus.Publish(command);

            // Assert
            Assert.True(await harness.Published.Any<ExecutePaymobRefundCommandEvent>());
            var consumerHarness = harness.GetConsumerHarness<ProcessPaymobRefundConsumer>();
            Assert.True(await consumerHarness.Consumed.Any<ExecutePaymobRefundCommandEvent>());

            var updatedRefund = await _dbContext.RefundRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == refundRequest.Id);
            Assert.NotNull(updatedRefund);
            Assert.Equal(RefundRequestStatus.Refunded, updatedRefund.Status);
            Assert.Equal("11223344", updatedRefund.ProviderRefundTransactionId);
            Assert.NotNull(updatedRefund.CompletedAtUtc);

            var updatedAttempt = await _dbContext.PaymentAttempts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == attempt.Id);
            Assert.NotNull(updatedAttempt);
            Assert.Equal(PaymentAttemptStatus.AutoRefunded, updatedAttempt.Status);

            var billingLog = await _dbContext.BillingLogs.AsNoTracking().FirstOrDefaultAsync(b => b.ProviderTransactionId == "987654321:refund");
            Assert.NotNull(billingLog);
            Assert.Equal(BillingPaymentStatus.Refunded, billingLog.PaymentStatus);
            Assert.Equal(-199.00m, billingLog.Amount);

            Assert.True(await harness.Published.Any<PaymentRefundedEvent>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
