using FitnessCalculationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessCalculationService.Persistence.Configurations;

public class UserPlanHistoryConfiguration : IEntityTypeConfiguration<UserPlanHistory>
{
    public void Configure(EntityTypeBuilder<UserPlanHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).HasMaxLength(50);
        builder.HasIndex(x => new { x.UserId, x.AssignedAt }).IsDescending(false, true);

        builder.Property(x => x.PlanId).HasMaxLength(20);
        builder.Property(x => x.Reason).HasMaxLength(255);
    }
}
