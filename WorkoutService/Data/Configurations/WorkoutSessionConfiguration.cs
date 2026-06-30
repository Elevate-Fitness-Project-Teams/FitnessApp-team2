using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutService.Data.Entities;

namespace WorkoutService.Data.Configurations;

public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
    public void Configure(EntityTypeBuilder<WorkoutSession> builder)
    {
        builder.HasKey(e => e.SessionId);
        builder.Property(e => e.SessionId).HasMaxLength(100);
        builder.HasIndex(e => e.UserId);
        builder.Property(e => e.Status).HasMaxLength(20);

        builder.HasOne(d => d.Workout)
            .WithMany(p => p.WorkoutSessions)
            .HasForeignKey(d => d.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
