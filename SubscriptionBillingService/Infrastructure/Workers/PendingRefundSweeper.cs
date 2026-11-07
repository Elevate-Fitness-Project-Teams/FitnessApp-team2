using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Contracts;
using SubscriptionBillingService.Domain.Enums;

namespace SubscriptionBillingService.Infrastructure.Workers;

public class PendingRefundSweeper : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingRefundSweeper> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public PendingRefundSweeper(
        IServiceProvider serviceProvider,
        ILogger<PendingRefundSweeper> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingRefundSweeper started with interval {Interval}", _checkInterval);

        using var timer = new PeriodicTimer(_checkInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepPendingRefundsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during pending refunds sweep.");
            }
        }
    }

    private async Task SweepPendingRefundsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var cutoff = DateTime.UtcNow.AddMinutes(-2);

        var staleRefunds = await dbContext.RefundRequests
            .Where(r => r.Status == RefundRequestStatus.Pending &&
                        r.RetryCount < 5 &&
                        r.CreatedAt <= cutoff)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (!staleRefunds.Any())
        {
            return;
        }

        _logger.LogInformation("Found {Count} pending refunds to re-dispatch.", staleRefunds.Count);

        foreach (var refund in staleRefunds)
        {
            await publishEndpoint.Publish(new ExecutePaymobRefundCommandEvent(
                refund.Id,
                refund.UserId,
                refund.PaymentAttemptId,
                refund.ProviderTransactionId,
                refund.AmountCents,
                refund.Currency,
                refund.Reason), cancellationToken);
        }
    }
}
