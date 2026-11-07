using MassTransit;
using Microsoft.EntityFrameworkCore;
using SubscriptionBillingService.Domain.Entities;

namespace SubscriptionBillingService.Common.Database;

public class SubscriptionDbContext : DbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<BillingLog> BillingLogs => Set<BillingLog>();
    public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubscriptionDbContext).Assembly);

        // MassTransit EF Core Outbox / Inbox state entities
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
