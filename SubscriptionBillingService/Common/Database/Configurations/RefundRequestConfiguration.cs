using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionBillingService.Domain.Entities;

namespace SubscriptionBillingService.Common.Database.Configurations;

public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(EntityTypeBuilder<RefundRequest> builder)
    {
        builder.ToTable("RefundRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PaymentAttemptId)
            .IsRequired();

        builder.Property(x => x.ProviderTransactionId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ProviderRefundTransactionId)
            .HasMaxLength(128);

        builder.Property(x => x.Currency)
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.RefundType)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(1024);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Enforce at most one refund request per original Paymob transaction
        builder.HasIndex(x => x.ProviderTransactionId)
            .IsUnique()
            .HasDatabaseName("UX_RefundRequests_ProviderTransactionId");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_RefundRequests_UserId");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_RefundRequests_Status");

        // Relationships
        builder.HasOne(x => x.PaymentAttempt)
            .WithOne(p => p.RefundRequest)
            .HasForeignKey<RefundRequest>(x => x.PaymentAttemptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
