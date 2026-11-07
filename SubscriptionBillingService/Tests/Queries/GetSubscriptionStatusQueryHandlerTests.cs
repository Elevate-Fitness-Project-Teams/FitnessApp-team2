using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Queries.GetSubscriptionStatus;
using Xunit;

namespace SubscriptionBillingService.Tests.Queries;

public class GetSubscriptionStatusQueryHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SubscriptionDbContext _dbContext;
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;

    public GetSubscriptionStatusQueryHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new SubscriptionDbContext(options);
        _dbContext.Database.EnsureCreated();

        _subscriptionRepo = new GenericRepository<UserSubscription>(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task Handle_WhenNoSubscriptionRowExists_ReturnsFreeTierDefault()
    {
        // Arrange
        var handler = new GetSubscriptionStatusQueryHandler(_subscriptionRepo);
        var userId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(new GetSubscriptionStatusQuery(userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Free", result.Value.Tier);
        Assert.Equal("None", result.Value.Status);
        Assert.False(result.Value.AutoRenew);
        Assert.Null(result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Handle_WhenActiveSubscriptionExists_ReturnsTierAndStatus()
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

        var handler = new GetSubscriptionStatusQueryHandler(_subscriptionRepo);

        // Act
        var result = await handler.Handle(new GetSubscriptionStatusQuery(userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Premium", result.Value.Tier);
        Assert.Equal("Active", result.Value.Status);
        Assert.True(result.Value.AutoRenew);
        Assert.Equal(expiresAt, result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionIsPastExpiration_ReturnsExpiredStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredAt = DateTime.UtcNow.AddDays(-2);

        var subscription = new UserSubscription
        {
            UserId = userId,
            Tier = SubscriptionTier.Premium,
            Status = SubscriptionStatus.Active,
            AutoRenew = false,
            ExpiresAtUtc = expiredAt
        };
        await _subscriptionRepo.AddAsync(subscription);
        await _dbContext.SaveChangesAsync();

        var handler = new GetSubscriptionStatusQueryHandler(_subscriptionRepo);

        // Act
        var result = await handler.Handle(new GetSubscriptionStatusQuery(userId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Free", result.Value.Tier);
        Assert.Equal("Expired", result.Value.Status);
        Assert.False(result.Value.AutoRenew);
    }
}
