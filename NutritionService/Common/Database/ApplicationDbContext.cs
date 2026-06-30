using Microsoft.EntityFrameworkCore;
using NutritionService.Models;

namespace NutritionService.Common.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MealPlan> MealPlans { get; set; } = null!;
    public DbSet<Meal> Meals { get; set; } = null!;
    public DbSet<Ingredient> Ingredients { get; set; } = null!;
    public DbSet<MealIngredient> MealIngredients { get; set; } = null!;
    public DbSet<MealPlanItem> MealPlanItems { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<MealTag> MealTags { get; set; } = null!;
    public DbSet<Allergen> Allergens { get; set; } = null!;
    public DbSet<MealAllergen> MealAllergens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
