using FitnessCalculationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessCalculationService.Persistence.Configurations;

public class FitnessPlanConfigConfiguration : IEntityTypeConfiguration<FitnessPlanConfig>
{
    public void Configure(EntityTypeBuilder<FitnessPlanConfig> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.Goal, x.Status }).IsUnique();

        builder.Property(x => x.Goal).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.PlanId).HasMaxLength(20);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
    }
}
