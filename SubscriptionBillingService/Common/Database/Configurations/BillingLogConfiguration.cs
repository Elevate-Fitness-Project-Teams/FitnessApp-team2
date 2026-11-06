using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionBillingService.Domain.Entities;

namespace SubscriptionBillingService.Common.Database.Configurations;

public class BillingLogConfiguration : IEntityTypeConfiguration<BillingLog>
{
    public void Configure(EntityTypeBuilder<BillingLog> builder)
    {
        builder.ToTable("BillingLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PaymentAttemptId)
            .IsRequired();

        builder.Property(x => x.PlanTier)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(x => x.PaymentProvider)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ProviderTransactionId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ProviderIntentionId)
            .HasMaxLength(128);

        builder.Property(x => x.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.RawPayloadHash)
            .HasMaxLength(128);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(512);

        // Enforce idempotent billing transaction recording
        builder.HasIndex(x => x.ProviderTransactionId)
            .IsUnique()
            .HasDatabaseName("UX_BillingLogs_ProviderTransactionId");

        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .HasDatabaseName("IX_BillingLogs_UserId_CreatedAt");

        // Relationships
        builder.HasOne(x => x.PaymentAttempt)
            .WithMany(p => p.BillingLogs)
            .HasForeignKey(x => x.PaymentAttemptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
