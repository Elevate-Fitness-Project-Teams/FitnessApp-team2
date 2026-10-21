using Microsoft.EntityFrameworkCore;
using NutritionService.Models;

namespace NutritionService.Common.Database;

public static class NutritionDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        if (await context.Meals.AnyAsync())
        {
            return;
        }

        var chickenBreast = new Ingredient { Id = Guid.Parse("33333333-3333-3333-3333-000000000001"), Name = "Chicken Breast" };
        var brownRice = new Ingredient { Id = Guid.Parse("33333333-3333-3333-3333-000000000002"), Name = "Brown Rice" };
        var broccoli = new Ingredient { Id = Guid.Parse("33333333-3333-3333-3333-000000000003"), Name = "Broccoli" };
        var rolledOats = new Ingredient { Id = Guid.Parse("33333333-3333-3333-3333-000000000004"), Name = "Rolled Oats" };
        var greekYogurt = new Ingredient { Id = Guid.Parse("33333333-3333-3333-3333-000000000005"), Name = "Greek Yogurt" };
        var salmon = new Ingredient { Id = Guid.Parse("33333333-3333-3333-3333-000000000006"), Name = "Atlantic Salmon" };

        await context.Ingredients.AddRangeAsync(chickenBreast, brownRice, broccoli, rolledOats, greekYogurt, salmon);

        var highProteinTag = new Tag { Id = Guid.Parse("33333333-3333-3333-3333-000000000010"), Name = "High Protein" };
        var mealPrepTag = new Tag { Id = Guid.Parse("33333333-3333-3333-3333-000000000011"), Name = "Meal Prep Friendly" };
        var balancedTag = new Tag { Id = Guid.Parse("33333333-3333-3333-3333-000000000012"), Name = "Balanced Macros" };

        await context.Tags.AddRangeAsync(highProteinTag, mealPrepTag, balancedTag);

        var dairyAllergen = new Allergen { Id = Guid.Parse("33333333-3333-3333-3333-000000000020"), Name = "Dairy" };
        var glutenAllergen = new Allergen { Id = Guid.Parse("33333333-3333-3333-3333-000000000021"), Name = "Gluten" };

        await context.Allergens.AddRangeAsync(dairyAllergen, glutenAllergen);

        var chickenBowl = new Meal
        {
            Id = Guid.Parse("33333333-3333-3333-3333-000000000030"),
            Name = "Grilled Chicken & Brown Rice Bowl",
            MealType = "Lunch",
            PrepTimeInMinutes = 25,
            Difficulty = "Easy",
            ImageUrl = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c",
            InstructionsJson = "[\"Season chicken breast with garlic, salt, and pepper.\",\"Grill over medium heat for 6-8 mins per side.\",\"Serve atop steamed brown rice and tender broccoli florets.\"]",
            Calories = 520,
            Protein = 45.0,
            Carbs = 55.0,
            Fats = 12.0,
            Fiber = 6.0
        };

        var powerOats = new Meal
        {
            Id = Guid.Parse("33333333-3333-3333-3333-000000000031"),
            Name = "Berry Greek Yogurt Power Oats",
            MealType = "Breakfast",
            PrepTimeInMinutes = 10,
            Difficulty = "Easy",
            ImageUrl = "https://images.unsplash.com/photo-1517673132405-a56a62b18caf",
            InstructionsJson = "[\"Cook rolled oats with water or milk.\",\"Stir in Greek yogurt until rich and creamy.\",\"Top with fresh berries and a drizzle of honey.\"]",
            Calories = 420,
            Protein = 30.0,
            Carbs = 52.0,
            Fats = 8.0,
            Fiber = 7.0
        };

        var salmonDinner = new Meal
        {
            Id = Guid.Parse("33333333-3333-3333-3333-000000000032"),
            Name = "Pan-Seared Atlantic Salmon & Asparagus",
            MealType = "Dinner",
            PrepTimeInMinutes = 20,
            Difficulty = "Medium",
            ImageUrl = "https://images.unsplash.com/photo-1467003909585-2f8a72700288",
            InstructionsJson = "[\"Heat olive oil in skillet.\",\"Sear salmon skin-side down for 4 mins, flip for 3 mins.\",\"Sauté asparagus in same pan with lemon.\"]",
            Calories = 580,
            Protein = 42.0,
            Carbs = 14.0,
            Fats = 38.0,
            Fiber = 5.0
        };

        await context.Meals.AddRangeAsync(chickenBowl, powerOats, salmonDinner);

        var mealPlan = new MealPlan
        {
            Id = Guid.Parse("33333333-3333-3333-3333-000000000040"),
            Name = "High-Protein Clean Fuel Plan",
            Description = "Balanced nutrition protocol supporting lean muscle synthesis, cognitive clarity, and sustained energy.",
            TargetCalorieRangeMin = 1800,
            TargetCalorieRangeMax = 2400
        };

        var mealPlanItems = new List<MealPlanItem>
        {
            new() { Id = Guid.NewGuid(), MealPlanId = mealPlan.Id, MealId = powerOats.Id, DayOfWeek = "Monday", MealTime = "Breakfast" },
            new() { Id = Guid.NewGuid(), MealPlanId = mealPlan.Id, MealId = chickenBowl.Id, DayOfWeek = "Monday", MealTime = "Lunch" },
            new() { Id = Guid.NewGuid(), MealPlanId = mealPlan.Id, MealId = salmonDinner.Id, DayOfWeek = "Monday", MealTime = "Dinner" }
        };

        await context.MealPlans.AddAsync(mealPlan);
        await context.MealPlanItems.AddRangeAsync(mealPlanItems);

        await context.SaveChangesAsync();
        logger.LogInformation("Successfully seeded default nutrition ingredients, tags, meals, and meal plan templates in NutritionService.");
    }
}
