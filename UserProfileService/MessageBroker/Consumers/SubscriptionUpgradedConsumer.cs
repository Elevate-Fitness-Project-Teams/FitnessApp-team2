using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubscriptionBillingService.Contracts;
using UserProfileService.Common.Database;

namespace UserProfileService.MessageBroker.Consumers;

public class SubscriptionUpgradedConsumer : IConsumer<SubscriptionUpgradedEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SubscriptionUpgradedConsumer> _logger;

    public SubscriptionUpgradedConsumer(
        ApplicationDbContext dbContext,
        ILogger<SubscriptionUpgradedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SubscriptionUpgradedEvent> context)
    {
        var msg = context.Message;
        var userIdStr = msg.UserId.ToString();

        _logger.LogInformation("Processing SubscriptionUpgradedEvent for UserId: {UserId}, Tier: {Tier}",
            msg.UserId, msg.PlanTier);

        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == userIdStr, context.CancellationToken);

        if (profile != null)
        {
            profile.IsPremiumCached = true;
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Successfully updated IsPremiumCached = true for user {UserId}", msg.UserId);
        }
        else
        {
            _logger.LogWarning("UserProfile not found for user {UserId} during subscription upgrade sync.", msg.UserId);
        }
    }
}
