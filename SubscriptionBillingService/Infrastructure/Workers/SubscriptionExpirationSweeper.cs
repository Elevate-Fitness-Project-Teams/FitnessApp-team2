using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Infrastructure.Caching;

namespace SubscriptionBillingService.Infrastructure.Workers;

public class SubscriptionExpirationSweeper : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpirationSweeper> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public SubscriptionExpirationSweeper(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionExpirationSweeper> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SubscriptionExpirationSweeper started with interval {Interval}", _checkInterval);

        using var timer = new PeriodicTimer(_checkInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepExpiredSubscriptionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during subscription expiration sweep.");
            }
        }
    }

    private async Task SweepExpiredSubscriptionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var tierCache = scope.ServiceProvider.GetRequiredService<ISubscriptionTierCache>();

        var now = DateTime.UtcNow;

        var expiredSubscriptions = await dbContext.UserSubscriptions
            .Where(s => s.Status == SubscriptionStatus.Active &&
                        s.ExpiresAtUtc.HasValue &&
                        s.ExpiresAtUtc.Value <= now)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (!expiredSubscriptions.Any())
        {
            return;
        }

        _logger.LogInformation("Found {Count} expired subscriptions to transition.", expiredSubscriptions.Count);

        foreach (var sub in expiredSubscriptions)
        {
            await unitOfWork.ExecuteAsync(async () =>
            {
                sub.Status = SubscriptionStatus.Expired;
                sub.UpdatedAt = DateTime.UtcNow;

                await tierCache.InvalidateTierAsync(sub.UserId, cancellationToken);

                await publishEndpoint.Publish(new SubscriptionExpiredEvent(
                    sub.UserId,
                    now), cancellationToken);
            }, cancellationToken);
        }
    }
}
