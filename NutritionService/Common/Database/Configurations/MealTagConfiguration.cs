using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutritionService.Models;

namespace NutritionService.Common.Database.Configurations;

public class MealTagConfiguration : IEntityTypeConfiguration<MealTag>
{
    public void Configure(EntityTypeBuilder<MealTag> builder)
    {
        builder.HasKey(mt => new { mt.MealId, mt.TagId });
    }
}
