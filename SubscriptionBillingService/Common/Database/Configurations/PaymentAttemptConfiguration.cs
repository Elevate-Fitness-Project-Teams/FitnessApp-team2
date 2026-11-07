using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionBillingService.Domain.Entities;

namespace SubscriptionBillingService.Common.Database.Configurations;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("PaymentAttempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.PlanTier)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ProviderIntentionId)
            .HasMaxLength(128);

        builder.Property(x => x.ProviderOrderId)
            .HasMaxLength(128);

        builder.Property(x => x.ProviderTransactionId)
            .HasMaxLength(128);

        builder.Property(x => x.ClientSecret)
            .HasMaxLength(256);

        builder.Property(x => x.CheckoutUrl)
            .HasMaxLength(512);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(512);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Enforce idempotent checkout creation per user
        builder.HasIndex(x => new { x.UserId, x.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("UX_PaymentAttempts_UserId_IdempotencyKey");

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_PaymentAttempts_UserId_Status");

        builder.HasIndex(x => x.ProviderIntentionId)
            .HasDatabaseName("IX_PaymentAttempts_ProviderIntentionId");

        builder.HasIndex(x => x.ProviderTransactionId)
            .HasDatabaseName("IX_PaymentAttempts_ProviderTransactionId");

        // Relationships
        builder.HasOne(x => x.UserSubscription)
            .WithMany(s => s.PaymentAttempts)
            .HasPrincipalKey(s => s.UserId)
            .HasForeignKey(x => x.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
