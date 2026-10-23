using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Data.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.IconUrl)
            .HasMaxLength(255);

        builder.Property(x => x.CriteriaType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Threshold)
            .IsRequired();
    }
}
