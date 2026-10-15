using FitnessCalculationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessCalculationService.Persistence.Configurations;

public class CalculatedMetricsConfiguration : IEntityTypeConfiguration<CalculatedMetrics>
{
    public void Configure(EntityTypeBuilder<CalculatedMetrics> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
        
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
