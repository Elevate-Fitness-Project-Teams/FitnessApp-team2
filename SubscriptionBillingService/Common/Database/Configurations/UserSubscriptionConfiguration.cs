using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionBillingService.Domain.Entities;

namespace SubscriptionBillingService.Common.Database.Configurations;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("UserSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Tier)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.LastProviderTransactionId)
            .HasMaxLength(128);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Enforce at most one subscription state record per user
        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("UX_UserSubscriptions_UserId");

        // Index for the background expiration sweeper
        builder.HasIndex(x => new { x.Status, x.ExpiresAtUtc })
            .HasDatabaseName("IX_UserSubscriptions_Status_ExpiresAtUtc");
    }
}
