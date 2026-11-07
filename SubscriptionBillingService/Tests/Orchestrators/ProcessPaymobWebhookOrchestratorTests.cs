using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.ProcessPaymobWebhook;
using SubscriptionBillingService.Features.Subscriptions.Orchestrators;
using SubscriptionBillingService.Infrastructure.Caching;
using SubscriptionBillingService.Infrastructure.Paymob;
using SubscriptionBillingService.Infrastructure.Paymob.Models;
using Xunit;

namespace SubscriptionBillingService.Tests.Orchestrators;

public class ProcessPaymobWebhookOrchestratorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SubscriptionDbContext _dbContext;
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IGenericRepository<PaymentAttempt> _attemptRepo;
    private readonly IGenericRepository<BillingLog> _billingLogRepo;
    private readonly IGenericRepository<RefundRequest> _refundRepo;
    private readonly IUnitOfWork _unitOfWork;
    private const string Secret = "secret_12345";
    private readonly PaymobHmacValidator _hmacValidator;

    public ProcessPaymobWebhookOrchestratorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new SubscriptionDbContext(options);
        _dbContext.Database.EnsureCreated();

        _subscriptionRepo = new GenericRepository<UserSubscription>(_dbContext);
        _attemptRepo = new GenericRepository<PaymentAttempt>(_dbContext);
        _billingLogRepo = new GenericRepository<BillingLog>(_dbContext);
        _refundRepo = new GenericRepository<RefundRequest>(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
        _hmacValidator = new PaymobHmacValidator(Secret);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    private class MockPublishEndpoint : IPublishEndpoint
    {
        public List<object> PublishedEvents { get; } = new();

        public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish<T>(object message, CancellationToken cancellationToken = default) where T : class
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish(object message, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish<T>(object message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish<T>(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => throw new NotImplementedException();
    }

    private class MockTierCache : ISubscriptionTierCache
    {
        public Dictionary<Guid, string> Cache { get; } = new();

        public Task SetTierAsync(Guid userId, string tier, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            Cache[userId] = tier;
            return Task.CompletedTask;
        }

        public Task<string?> GetTierAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            Cache.TryGetValue(userId, out var tier);
            return Task.FromResult<string?>(tier);
        }

        public Task InvalidateTierAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            Cache.Remove(userId);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_WhenValidFirstPayment_ActivatesPremiumAndPublishesUpgradeEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var attempt = new PaymentAttempt
        {
            UserId = userId,
            IdempotencyKey = "attempt-1",
            PlanTier = SubscriptionTier.Premium,
            DurationMonths = 1,
            AmountCents = 19900,
            Currency = "EGP",
            Status = PaymentAttemptStatus.PendingRedirect
        };
        await _attemptRepo.AddAsync(attempt);
        await _dbContext.SaveChangesAsync();

        var mockPublish = new MockPublishEndpoint();
        var mockCache = new MockTierCache();

        var orchestrator = new ProcessPaymobWebhookOrchestrator(
            _hmacValidator, _subscriptionRepo, _attemptRepo, _billingLogRepo, _refundRepo,
            _unitOfWork, mockPublish, mockCache, NullLogger<ProcessPaymobWebhookOrchestrator>.Instance);

        var obj = new PaymobTransactionObject
        {
            Id = 12345678,
            AmountCents = 19900,
            Currency = "EGP",
            Success = true,
            SpecialReference = attempt.Id.ToString()
        };
        var hmac = _hmacValidator.ComputeHmac(obj);

        var command = new ProcessPaymobWebhookCommand(new PaymobWebhookPayload { Obj = obj }, hmac);

        // Act
        var result = await orchestrator.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var updatedAttempt = await _attemptRepo.GetByIdAsync(attempt.Id);
        Assert.NotNull(updatedAttempt);
        Assert.Equal(PaymentAttemptStatus.Succeeded, updatedAttempt.Status);

        var subscription = await _subscriptionRepo.FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(subscription);
        Assert.Equal(SubscriptionTier.Premium, subscription.Tier);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.NotNull(subscription.ExpiresAtUtc);

        var log = await _billingLogRepo.FirstOrDefaultAsync(b => b.ProviderTransactionId == "12345678");
        Assert.NotNull(log);
        Assert.Equal(BillingPaymentStatus.Success, log.PaymentStatus);

        Assert.Single(mockPublish.PublishedEvents);
        var upgradeEvent = Assert.IsType<SubscriptionUpgradedEvent>(mockPublish.PublishedEvents[0]);
        Assert.Equal(userId, upgradeEvent.UserId);

        Assert.Equal("Premium", mockCache.Cache[userId]);
    }

    [Fact]
    public async Task Handle_WhenDuplicatePaymentOnAlreadyActiveUser_TriggersAtomicAutoRefund()
    {
        // Arrange
        var userId = Guid.NewGuid();
        // User is ALREADY active Premium!
        var existingActiveSub = new UserSubscription
        {
            UserId = userId,
            Tier = SubscriptionTier.Premium,
            Status = SubscriptionStatus.Active,
            ExpiresAtUtc = DateTime.UtcNow.AddMonths(1)
        };
        await _subscriptionRepo.AddAsync(existingActiveSub);

        var attempt = new PaymentAttempt
        {
            UserId = userId,
            IdempotencyKey = "dup-attempt",
            PlanTier = SubscriptionTier.Premium,
            DurationMonths = 1,
            AmountCents = 19900,
            Currency = "EGP",
            Status = PaymentAttemptStatus.PendingRedirect
        };
        await _attemptRepo.AddAsync(attempt);
        await _dbContext.SaveChangesAsync();

        var mockPublish = new MockPublishEndpoint();
        var mockCache = new MockTierCache();

        var orchestrator = new ProcessPaymobWebhookOrchestrator(
            _hmacValidator, _subscriptionRepo, _attemptRepo, _billingLogRepo, _refundRepo,
            _unitOfWork, mockPublish, mockCache, NullLogger<ProcessPaymobWebhookOrchestrator>.Instance);

        var obj = new PaymobTransactionObject
        {
            Id = 88776655,
            AmountCents = 19900,
            Currency = "EGP",
            Success = true,
            SpecialReference = attempt.Id.ToString()
        };
        var hmac = _hmacValidator.ComputeHmac(obj);

        var command = new ProcessPaymobWebhookCommand(new PaymobWebhookPayload { Obj = obj }, hmac);

        // Act
        var result = await orchestrator.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // 1. Attempt marked DuplicateChargePendingRefund
        var updatedAttempt = await _attemptRepo.GetByIdAsync(attempt.Id);
        Assert.NotNull(updatedAttempt);
        Assert.Equal(PaymentAttemptStatus.DuplicateChargePendingRefund, updatedAttempt.Status);

        // 2. BillingLog marked DuplicateChargeDetected
        var log = await _billingLogRepo.FirstOrDefaultAsync(b => b.ProviderTransactionId == "88776655");
        Assert.NotNull(log);
        Assert.Equal(BillingPaymentStatus.DuplicateChargeDetected, log.PaymentStatus);

        // 3. RefundRequest row created with Pending status
        var refund = await _refundRepo.FirstOrDefaultAsync(r => r.ProviderTransactionId == "88776655");
        Assert.NotNull(refund);
        Assert.Equal(RefundRequestStatus.Pending, refund.Status);
        Assert.Equal(19900, refund.AmountCents);

        // 4. Staged ExecutePaymobRefundCommandEvent into Outbox!
        Assert.Single(mockPublish.PublishedEvents);
        var refundCommand = Assert.IsType<ExecutePaymobRefundCommandEvent>(mockPublish.PublishedEvents[0]);
        Assert.Equal(userId, refundCommand.UserId);
        Assert.Equal("88776655", refundCommand.ProviderTransactionId);
        Assert.Equal(19900, refundCommand.AmountCents);
    }

    [Fact]
    public async Task Handle_WhenInvalidHmac_ReturnsUnauthorized()
    {
        var orchestrator = new ProcessPaymobWebhookOrchestrator(
            _hmacValidator, _subscriptionRepo, _attemptRepo, _billingLogRepo, _refundRepo,
            _unitOfWork, new MockPublishEndpoint(), new MockTierCache(), NullLogger<ProcessPaymobWebhookOrchestrator>.Instance);

        var obj = new PaymobTransactionObject { Id = 999 };
        var command = new ProcessPaymobWebhookCommand(new PaymobWebhookPayload { Obj = obj }, "invalid_hmac_hex");

        var result = await orchestrator.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTH_SIGNATURE_INVALID", result.Error.Code);
    }
}
