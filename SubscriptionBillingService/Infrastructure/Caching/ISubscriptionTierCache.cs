namespace SubscriptionBillingService.Infrastructure.Caching;

public interface ISubscriptionTierCache
{
    Task SetTierAsync(Guid userId, string tier, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<string?> GetTierAsync(Guid userId, CancellationToken cancellationToken = default);
    Task InvalidateTierAsync(Guid userId, CancellationToken cancellationToken = default);
}
