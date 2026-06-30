using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutritionService.Models;

namespace NutritionService.Common.Database.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(i => i.MealIngredients)
            .WithOne(mi => mi.Ingredient)
            .HasForeignKey(mi => mi.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.Name)
            .IsUnique()
            .HasDatabaseName("IX_Ingredients_Name");
    }
}
