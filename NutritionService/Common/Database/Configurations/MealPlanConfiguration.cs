using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutritionService.Models;

namespace NutritionService.Common.Database.Configurations;

public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(mp => mp.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(mp => mp.TargetCalorieRangeMin)
            .IsRequired();

        builder.Property(mp => mp.TargetCalorieRangeMax)
            .IsRequired();

        // Navigation properties
        builder.HasMany(mp => mp.MealPlanItems)
            .WithOne(mpi => mpi.MealPlan)
            .HasForeignKey(mpi => mpi.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for US 6.5
        builder.HasIndex(mp => new { mp.TargetCalorieRangeMin, mp.TargetCalorieRangeMax })
            .HasDatabaseName("IX_MealPlans_CalorieRanges");
    }
}
