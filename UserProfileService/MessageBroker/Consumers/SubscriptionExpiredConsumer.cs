using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubscriptionBillingService.Contracts;
using UserProfileService.Common.Database;

namespace UserProfileService.MessageBroker.Consumers;

public class SubscriptionExpiredConsumer : IConsumer<SubscriptionExpiredEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SubscriptionExpiredConsumer> _logger;

    public SubscriptionExpiredConsumer(
        ApplicationDbContext dbContext,
        ILogger<SubscriptionExpiredConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SubscriptionExpiredEvent> context)
    {
        var msg = context.Message;
        var userIdStr = msg.UserId.ToString();

        _logger.LogInformation("Processing SubscriptionExpiredEvent for UserId: {UserId}", msg.UserId);

        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == userIdStr, context.CancellationToken);

        if (profile != null)
        {
            profile.IsPremiumCached = false;
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Successfully updated IsPremiumCached = false for user {UserId}", msg.UserId);
        }
        else
        {
            _logger.LogWarning("UserProfile not found for user {UserId} during subscription expired sync.", msg.UserId);
        }
    }
}
