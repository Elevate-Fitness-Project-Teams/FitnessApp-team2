using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutService.Data.Entities;

namespace WorkoutService.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Category);
        builder.Property(e => e.Category).HasMaxLength(50);
        builder.Property(e => e.Difficulty).HasMaxLength(20);

        builder.HasOne(d => d.WorkoutPlan)
            .WithMany(p => p.Workouts)
            .HasForeignKey(d => d.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
