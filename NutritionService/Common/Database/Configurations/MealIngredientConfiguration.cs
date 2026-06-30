using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutritionService.Models;

namespace NutritionService.Common.Database.Configurations;

public class MealIngredientConfiguration : IEntityTypeConfiguration<MealIngredient>
{
    public void Configure(EntityTypeBuilder<MealIngredient> builder)
    {
        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.Amount)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(mi => new { mi.MealId, mi.IngredientId })
            .IsUnique()
            .HasDatabaseName("UQ_MealIngredients_MealId_IngredientId");
    }
}
