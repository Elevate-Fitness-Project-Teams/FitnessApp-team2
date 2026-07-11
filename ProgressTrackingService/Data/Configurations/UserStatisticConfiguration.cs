using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Data.Configurations;

public class UserStatisticConfiguration : IEntityTypeConfiguration<UserStatistic>
{
    public void Configure(EntityTypeBuilder<UserStatistic> builder)
    {
        builder.HasKey(x => x.UserId);
        
        builder.Property(x => x.TotalWorkouts)
            .HasDefaultValue(0);
            
        builder.Property(x => x.TotalCaloriesBurned)
            .HasDefaultValue(0);
            
        builder.Property(x => x.TotalWeightLost)
            .HasDefaultValue(0);
    }
}
