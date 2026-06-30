using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutritionService.Models;

namespace NutritionService.Common.Database.Configurations;

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.MealType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.PrepTimeInMinutes)
            .IsRequired();

        builder.Property(m => m.Difficulty)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.ImageUrl)
            .HasMaxLength(500);

        builder.Property(m => m.InstructionsJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.VariationsJson)
            .HasColumnType("nvarchar(max)");

        // Flattened Nutrition Facts
        builder.Property(m => m.Calories).IsRequired();
        builder.Property(m => m.Protein).IsRequired();
        builder.Property(m => m.Carbs).IsRequired();
        builder.Property(m => m.Fats).IsRequired();
        builder.Property(m => m.Fiber).IsRequired();

        // 1-to-many relationship with MealIngredients
        builder.HasMany(m => m.MealIngredients)
            .WithOne(mi => mi.Meal)
            .HasForeignKey(mi => mi.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-to-many relationship with MealPlanItems
        builder.HasMany(m => m.MealPlanItems)
            .WithOne(mpi => mpi.Meal)
            .HasForeignKey(mpi => mpi.MealId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1-to-many relationship with MealTags
        builder.HasMany(m => m.MealTags)
            .WithOne(mt => mt.Meal)
            .HasForeignKey(mt => mt.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1-to-many relationship with MealAllergens
        builder.HasMany(m => m.MealAllergens)
            .WithOne(ma => ma.Meal)
            .HasForeignKey(ma => ma.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite Index for US 8.1
        builder.HasIndex(m => new { m.MealType, m.Calories, m.Protein })
            .HasDatabaseName("IX_Meals_MealType_Calories_Protein");
    }
}
