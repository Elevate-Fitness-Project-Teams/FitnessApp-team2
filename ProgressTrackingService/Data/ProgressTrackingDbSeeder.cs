using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Data;

public static class ProgressTrackingDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        var testUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var hasWeight = await context.WeightHistories.AnyAsync(w => w.UserId == testUserId);
        if (!hasWeight)
        {
            var weightEntries = new List<WeightHistory>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    Weight = 83.5,
                    Date = DateTime.UtcNow.AddDays(-14),
                    Notes = "Initial onboarding weigh-in"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    Weight = 82.5,
                    Date = DateTime.UtcNow.AddDays(-7),
                    Notes = "Week 1 progress check-in"
                }
            };

            var achievements = new List<Achievement>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    Name = "First Step",
                    Description = "Registered profile and submitted initial fitness measurements.",
                    IconUrl = "https://cdn.elevatefitness.com/icons/achievements/first-step.png"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    Name = "Consistency Starter",
                    Description = "Completed 3 workouts in your first week of training.",
                    IconUrl = "https://cdn.elevatefitness.com/icons/achievements/consistency.png"
                }
            };

            var streak = new Streak
            {
                UserId = testUserId,
                CurrentStreak = 3,
                LongestStreak = 5,
                LastWorkoutDate = DateTime.UtcNow.AddDays(-1)
            };

            var stats = new UserStatistic
            {
                UserId = testUserId,
                TotalWorkouts = 5,
                TotalCaloriesBurned = 1750,
                TotalWeightLost = 1.0,
                UpdatedAt = DateTime.UtcNow
            };

            await context.WeightHistories.AddRangeAsync(weightEntries);
            await context.Achievements.AddRangeAsync(achievements);
            await context.Streaks.AddAsync(streak);
            await context.UserStatistics.AddAsync(stats);

            await context.SaveChangesAsync();
            logger.LogInformation("Successfully seeded baseline weight records, streak, and statistics for test user in ProgressTrackingService.");
        }
    }
}
