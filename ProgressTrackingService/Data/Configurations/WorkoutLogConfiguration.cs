using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Data.Configurations;

public class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.SessionId);
        
        builder.Property(x => x.Difficulty).HasConversion<string>();
        
        builder.Property(x => x.SessionId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Notes)
            .HasMaxLength(1000);
            
        builder.HasMany(x => x.Exercises)
            .WithOne(x => x.WorkoutLog)
            .HasForeignKey(x => x.WorkoutLogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}