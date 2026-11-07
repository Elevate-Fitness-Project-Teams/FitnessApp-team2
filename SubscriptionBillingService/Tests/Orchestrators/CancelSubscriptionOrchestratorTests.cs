using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.CancelSubscription;
using SubscriptionBillingService.Features.Subscriptions.Orchestrators;
using Xunit;

namespace SubscriptionBillingService.Tests.Orchestrators;

public class CancelSubscriptionOrchestratorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SubscriptionDbContext _dbContext;
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSubscriptionOrchestratorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new SubscriptionDbContext(options);
        _dbContext.Database.EnsureCreated();

        _subscriptionRepo = new GenericRepository<UserSubscription>(_dbContext);
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

    [Fact]
    public async Task Handle_WhenNoActiveSubscription_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockPublish = new MockPublishEndpoint();
        var orchestrator = new CancelSubscriptionOrchestrator(
            _subscriptionRepo, _unitOfWork, mockPublish, NullLogger<CancelSubscriptionOrchestrator>.Instance);

        // Act
        var result = await orchestrator.Handle(new CancelSubscriptionCommand(userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("RES_NOT_FOUND", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenActiveSubscriptionExists_TurnsOffAutoRenewWithoutTruncatingExpiresAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMonths(1);

        var subscription = new UserSubscription
        {
            UserId = userId,
            Tier = SubscriptionTier.Premium,
            Status = SubscriptionStatus.Active,
            AutoRenew = true,
            ExpiresAtUtc = expiresAt
        };
        await _subscriptionRepo.AddAsync(subscription);
        await _dbContext.SaveChangesAsync();

        var mockPublish = new MockPublishEndpoint();
        var orchestrator = new CancelSubscriptionOrchestrator(
            _subscriptionRepo, _unitOfWork, mockPublish, NullLogger<CancelSubscriptionOrchestrator>.Instance);

        // Act
        var result = await orchestrator.Handle(new CancelSubscriptionCommand(userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.AutoRenew);
        Assert.Equal(expiresAt, result.Value.ExpiresAt);
        Assert.Equal("Cancelled", result.Value.Status);

        var updated = await _subscriptionRepo.FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(updated);
        Assert.False(updated.AutoRenew);
        Assert.Equal(SubscriptionStatus.Cancelled, updated.Status);
        Assert.Equal(expiresAt, updated.ExpiresAtUtc); // Unchanged!

        Assert.Single(mockPublish.PublishedEvents);
        var cancelEvent = Assert.IsType<SubscriptionCancelledEvent>(mockPublish.PublishedEvents[0]);
        Assert.Equal(userId, cancelEvent.UserId);
        Assert.Equal(expiresAt, cancelEvent.EffectiveExpiresAtUtc);
    }
}
