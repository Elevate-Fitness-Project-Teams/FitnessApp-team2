using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Data.Configurations;

public class StreakConfiguration : IEntityTypeConfiguration<Streak>
{
    public void Configure(EntityTypeBuilder<Streak> builder)
    {
        builder.HasKey(x => x.UserId);
        
        builder.Property(x => x.CurrentStreak)
            .HasDefaultValue(0);
            
        builder.Property(x => x.LongestStreak)
            .HasDefaultValue(0);
    }
}
