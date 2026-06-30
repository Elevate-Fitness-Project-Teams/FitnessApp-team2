using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutritionService.Models;

namespace NutritionService.Common.Database.Configurations;

public class MealPlanItemConfiguration : IEntityTypeConfiguration<MealPlanItem>
{
    public void Configure(EntityTypeBuilder<MealPlanItem> builder)
    {
        builder.HasKey(mpi => mpi.Id);

        builder.Property(mpi => mpi.DayOfWeek)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(mpi => mpi.MealTime)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(mpi => new { mpi.MealPlanId, mpi.DayOfWeek, mpi.MealTime })
            .IsUnique()
            .HasDatabaseName("UQ_MealPlanItems_PlanId_Day_Time");
    }
}
