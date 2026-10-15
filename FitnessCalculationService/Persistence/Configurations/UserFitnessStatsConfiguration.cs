using FitnessCalculationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessCalculationService.Persistence.Configurations;

public class UserFitnessStatsConfiguration : IEntityTypeConfiguration<UserFitnessStats>
{
    public void Configure(EntityTypeBuilder<UserFitnessStats> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId).IsRequired();
        
        // Non-unique index on (UserId, RecordedAt DESC) for efficient retrieval of the latest biometrics snapshot
        builder.HasIndex(x => new { x.UserId, x.RecordedAt }).IsDescending(false, true);
        
        builder.Property(x => x.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Goal).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.ActivityLevel).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Source).HasMaxLength(50);
    }
}
