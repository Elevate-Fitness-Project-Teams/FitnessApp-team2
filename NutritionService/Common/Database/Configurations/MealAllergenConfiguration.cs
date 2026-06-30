using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutritionService.Models;

namespace NutritionService.Common.Database.Configurations;

public class MealAllergenConfiguration : IEntityTypeConfiguration<MealAllergen>
{
    public void Configure(EntityTypeBuilder<MealAllergen> builder)
    {
        builder.HasKey(ma => new { ma.MealId, ma.AllergenId });
    }
}
