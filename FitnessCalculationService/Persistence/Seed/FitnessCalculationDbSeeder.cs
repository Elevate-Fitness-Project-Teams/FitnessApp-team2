using FitnessCalculationService.Domain.Entities;
using FitnessCalculationService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessCalculationService.Persistence.Seed;

public static class FitnessCalculationDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FceDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FceDbContext>>();

        var testUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var hasStats = await context.UserFitnessStats.AnyAsync(s => s.UserId == testUserId);
        if (!hasStats)
        {
            var stats = new UserFitnessStats
            {
                Id = Guid.NewGuid(),
                UserId = testUserId,
                Weight = 82.5,
                Height = 180.0,
                Age = 28,
                Gender = Gender.Male,
                Goal = FitnessGoal.LoseWeight,
                ActivityLevel = ActivityLevel.Intermediate,
                RecordedAt = DateTime.UtcNow,
                Source = "user"
            };

            var metrics = new CalculatedMetrics
            {
                Id = Guid.NewGuid(),
                UserId = testUserId,
                Bmr = 1812.5,
                Tdee = 2809.38,
                CalorieTarget = 2309.38,
                Status = CalorieStatus.Normal,
                CalculatedAt = DateTime.UtcNow,
                SourceStatsId = stats.Id
            };

            var planConfig = await context.FitnessPlanConfigs
                .FirstOrDefaultAsync(p => p.PlanId == "LW-N");

            if (planConfig != null)
            {
                var assignedPlan = new UserAssignedPlan
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    FitnessPlanConfigId = planConfig.Id,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow
                };

                var planHistory = new UserPlanHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    PlanId = planConfig.PlanId,
                    PreviousPlanId = null,
                    Goal = FitnessGoal.LoseWeight,
                    Status = CalorieStatus.Normal,
                    PreviousStatus = null,
                    Bmr = metrics.Bmr,
                    Tdee = metrics.Tdee,
                    CalorieTarget = metrics.CalorieTarget,
                    AssignedAt = DateTime.UtcNow,
                    Reason = "Initial baseline plan assignment for test user onboarding",
                    TriggeredBy = "user",
                    AssignedPlanId = assignedPlan.Id
                };

                await context.UserAssignedPlans.AddAsync(assignedPlan);
                await context.UserPlanHistories.AddAsync(planHistory);
            }

            await context.UserFitnessStats.AddAsync(stats);
            await context.CalculatedMetrics.AddAsync(metrics);

            await context.SaveChangesAsync();
            logger.LogInformation("Successfully seeded baseline test stats, calculated metrics, and active plan in FitnessCalculationService.");
        }
    }
}
