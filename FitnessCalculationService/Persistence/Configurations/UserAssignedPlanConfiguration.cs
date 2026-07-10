using FitnessCalculationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessCalculationService.Persistence.Configurations;

public class UserAssignedPlanConfiguration : IEntityTypeConfiguration<UserAssignedPlan>
{
    public void Configure(EntityTypeBuilder<UserAssignedPlan> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.FitnessPlanConfig)
               .WithMany(x => x.AssignedUsers)
               .HasForeignKey(x => x.FitnessPlanConfigId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.UserId).HasMaxLength(50);
        builder.HasIndex(x => x.UserId).HasFilter("[IsActive] = 1").IsUnique();
    }
}
