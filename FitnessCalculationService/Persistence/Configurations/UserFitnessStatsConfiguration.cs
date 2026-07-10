using FitnessCalculationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessCalculationService.Persistence.Configurations;

public class UserFitnessStatsConfiguration : IEntityTypeConfiguration<UserFitnessStats>
{
    public void Configure(EntityTypeBuilder<UserFitnessStats> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId).HasMaxLength(50);
        builder.HasIndex(x => x.UserId).IsUnique();
        
        builder.Property(x => x.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Goal).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.ActivityLevel).HasConversion<string>().HasMaxLength(50);
    }
}
