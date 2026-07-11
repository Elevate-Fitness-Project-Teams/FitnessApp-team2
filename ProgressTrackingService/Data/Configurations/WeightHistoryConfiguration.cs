using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Data.Configurations;

public class WeightHistoryConfiguration : IEntityTypeConfiguration<WeightHistory>
{
    public void Configure(EntityTypeBuilder<WeightHistory> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => x.UserId);
        
        builder.Property(x => x.Notes)
            .HasMaxLength(500);
    }
}
