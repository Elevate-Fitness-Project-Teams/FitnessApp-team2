using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutService.Data.Entities;

namespace WorkoutService.Data.Configurations;

public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.ExternalPlanId).IsUnique();
        builder.Property(e => e.ExternalPlanId).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Goal).HasMaxLength(50);
        builder.Property(e => e.Status).HasMaxLength(20);
        builder.Property(e => e.Difficulty).HasMaxLength(20);
    }
}
