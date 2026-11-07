using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.RefundSubscription;
using SubscriptionBillingService.Features.Subscriptions.Orchestrators;
using SubscriptionBillingService.Infrastructure.Caching;
using Xunit;

namespace SubscriptionBillingService.Tests.Orchestrators;

public class RefundSubscriptionOrchestratorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SubscriptionDbContext _dbContext;
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IGenericRepository<PaymentAttempt> _attemptRepo;
    private readonly IGenericRepository<RefundRequest> _refundRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RefundSubscriptionOrchestratorTests()
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
        _refundRepo = new GenericRepository<RefundRequest>(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
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

        public Task Publish(object message, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<T>(object message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => throw new NotImplementedException();
    }

    private class MockTierCache : ISubscriptionTierCache
    {
        public bool Invalidated { get; private set; }

        public Task SetTierAsync(Guid userId, string tier, TimeSpan ttl, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<string?> GetTierAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<string?>("Premium");
        public Task InvalidateTierAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            Invalidated = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Handle_WhenValidRefund_RevokesSubscriptionAndStagesRefundCommand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var txId = "99887766";

        var subscription = new UserSubscription
        {
            UserId = userId,
            Tier = SubscriptionTier.Premium,
            Status = SubscriptionStatus.Active,
            ExpiresAtUtc = DateTime.UtcNow.AddMonths(1),
            LastProviderTransactionId = txId
        };
        await _subscriptionRepo.AddAsync(subscription);

        var attempt = new PaymentAttempt
        {
            UserId = userId,
            IdempotencyKey = "tx-attempt-1",
            ProviderTransactionId = txId,
            AmountCents = 19900,
            Currency = "EGP",
            Status = PaymentAttemptStatus.Succeeded
        };
        await _attemptRepo.AddAsync(attempt);
        await _dbContext.SaveChangesAsync();

        var mockPublish = new MockPublishEndpoint();
        var mockCache = new MockTierCache();
        var orchestrator = new RefundSubscriptionOrchestrator(
            _subscriptionRepo, _attemptRepo, _refundRepo, _unitOfWork,
            mockPublish, mockCache, NullLogger<RefundSubscriptionOrchestrator>.Instance);

        // Act
        var result = await orchestrator.Handle(
            new RefundSubscriptionCommand(userId, "Customer unsatisfied"), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(199.00m, result.Value.Amount);
        Assert.Equal("Pending", result.Value.Status);

        var updatedSub = await _subscriptionRepo.FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(updatedSub);
        Assert.Equal(SubscriptionTier.Free, updatedSub.Tier);
        Assert.Equal(SubscriptionStatus.Refunded, updatedSub.Status);
        Assert.True(mockCache.Invalidated);

        var refund = await _refundRepo.FirstOrDefaultAsync(r => r.ProviderTransactionId == txId);
        Assert.NotNull(refund);
        Assert.Equal(RefundRequestStatus.Pending, refund.Status);
        Assert.Equal(RefundType.ExplicitSubscriptionRefund, refund.RefundType);

        Assert.Equal(2, mockPublish.PublishedEvents.Count);
        Assert.Contains(mockPublish.PublishedEvents, e => e is ExecutePaymobRefundCommandEvent);
        Assert.Contains(mockPublish.PublishedEvents, e => e is SubscriptionExpiredEvent);
    }
}
