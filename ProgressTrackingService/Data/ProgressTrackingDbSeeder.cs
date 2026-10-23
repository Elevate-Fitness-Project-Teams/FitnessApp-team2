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

        // 1. Seed Master Achievement Catalog
        var masterAchievements = new List<Achievement>
        {
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                Code = "FIRST_WORKOUT",
                Name = "First Step",
                Description = "Completed your very first workout session.",
                IconUrl = "https://cdn.elevatefitness.com/icons/achievements/first-step.png",
                CriteriaType = "TotalWorkouts",
                Threshold = 1
            },
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                Code = "WORKOUTS_10",
                Name = "Getting Serious",
                Description = "Completed 10 workout sessions.",
                IconUrl = "https://cdn.elevatefitness.com/icons/achievements/getting-serious.png",
                CriteriaType = "TotalWorkouts",
                Threshold = 10
            },
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                Code = "WORKOUTS_50",
                Name = "Dedicated Athlete",
                Description = "Completed 50 workout sessions.",
                IconUrl = "https://cdn.elevatefitness.com/icons/achievements/dedicated.png",
                CriteriaType = "TotalWorkouts",
                Threshold = 50
            },
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000004"),
                Code = "STREAK_3",
                Name = "Consistency Starter",
                Description = "Maintained an active workout streak for 3 consecutive days.",
                IconUrl = "https://cdn.elevatefitness.com/icons/achievements/consistency.png",
                CriteriaType = "CurrentStreak",
                Threshold = 3
            },
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000005"),
                Code = "STREAK_7",
                Name = "One-Week Warrior",
                Description = "Maintained an active workout streak for 7 consecutive days.",
                IconUrl = "https://cdn.elevatefitness.com/icons/achievements/streak-7.png",
                CriteriaType = "CurrentStreak",
                Threshold = 7
            },
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000006"),
                Code = "CALORIES_1K",
                Name = "Calorie Crusher",
                Description = "Burned over 1,000 total calories through completed workouts.",
                IconUrl = "https://cdn.elevatefitness.com/icons/achievements/calorie-crusher.png",
                CriteriaType = "TotalCaloriesBurned",
                Threshold = 1000
            },
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000007"),
                Code = "WEIGHT_LOST_1",
                Name = "First Kilo",
                Description = "Lost your first kilogram on your fitness journey.",
                IconUrl = "https://cdn.elevatefitness.com/icons/achievements/weight-lost-1.png",
                CriteriaType = "TotalWeightLost",
                Threshold = 1.0
            }
        };

        foreach (var achievement in masterAchievements)
        {
            var exists = await context.Achievements.AnyAsync(a => a.Code == achievement.Code);
            if (!exists)
            {
                await context.Achievements.AddAsync(achievement);
            }
        }
        await context.SaveChangesAsync();

        // 2. Seed Baseline Data for Unified Test User
        var testUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Biometrics
        var hasBiometrics = await context.UserBiometrics.AnyAsync(b => b.UserId == testUserId);
        if (!hasBiometrics)
        {
            await context.UserBiometrics.AddAsync(new UserBiometrics
            {
                UserId = testUserId,
                HeightCm = 180.0,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Weight History
        var hasWeight = await context.WeightHistories.AnyAsync(w => w.UserId == testUserId);
        if (!hasWeight)
        {
            var weightEntries = new List<WeightHistory>
            {
                new()
                {
                    Id = Guid.Parse("b0000000-0000-0000-0000-000000000001"),
                    UserId = testUserId,
                    Weight = 83.5,
                    Date = DateTime.UtcNow.AddDays(-14),
                    Notes = "Initial onboarding weigh-in"
                },
                new()
                {
                    Id = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                    UserId = testUserId,
                    Weight = 82.5,
                    Date = DateTime.UtcNow.AddDays(-7),
                    Notes = "Week 1 progress check-in"
                }
            };
            await context.WeightHistories.AddRangeAsync(weightEntries);
        }

        // Streak
        var hasStreak = await context.Streaks.AnyAsync(s => s.UserId == testUserId);
        if (!hasStreak)
        {
            await context.Streaks.AddAsync(new Streak
            {
                UserId = testUserId,
                CurrentStreak = 3,
                LongestStreak = 5,
                LastWorkoutDate = DateTime.UtcNow.AddDays(-1)
            });
        }

        // Statistics
        var hasStats = await context.UserStatistics.AnyAsync(s => s.UserId == testUserId);
        if (!hasStats)
        {
            await context.UserStatistics.AddAsync(new UserStatistic
            {
                UserId = testUserId,
                TotalWorkouts = 5,
                TotalCaloriesBurned = 1750,
                TotalMinutes = 150,
                InitialWeight = 83.5,
                CurrentWeight = 82.5,
                TotalWeightLost = 1.0,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // User Achievements
        var hasUserAchievements = await context.UserAchievements.AnyAsync(ua => ua.UserId == testUserId);
        if (!hasUserAchievements)
        {
            var firstWorkoutBadge = await context.Achievements.FirstOrDefaultAsync(a => a.Code == "FIRST_WORKOUT");
            var consistencyBadge = await context.Achievements.FirstOrDefaultAsync(a => a.Code == "STREAK_3");

            if (firstWorkoutBadge != null)
            {
                await context.UserAchievements.AddAsync(new UserAchievement
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    AchievementId = firstWorkoutBadge.Id,
                    EarnedAt = DateTime.UtcNow.AddDays(-5)
                });
            }

            if (consistencyBadge != null)
            {
                await context.UserAchievements.AddAsync(new UserAchievement
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    AchievementId = consistencyBadge.Id,
                    EarnedAt = DateTime.UtcNow.AddDays(-1)
                });
            }
        }

        // Workout Logs for timeline and dashboard testing
        var hasLogs = await context.WorkoutLogs.AnyAsync(wl => wl.UserId == testUserId);
        if (!hasLogs)
        {
            var workoutLog = new WorkoutLog
            {
                Id = Guid.Parse("c0000000-0000-0000-0000-000000000001"),
                UserId = testUserId,
                WorkoutId = Guid.Parse("d0000000-0000-0000-0000-000000000001"),
                SessionId = "seed-session-1111",
                DurationInMinutes = 35,
                CaloriesBurned = 350,
                Difficulty = WorkoutDifficulty.Intermediate,
                Rating = 5,
                Notes = "Excellent chest and core session",
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                Exercises = new List<WorkoutLogExercise>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ExerciseId = Guid.NewGuid(),
                        SetsCompleted = 3,
                        RepsCompleted = 12,
                        WeightUsed = 60,
                        Completed = true
                    }
                }
            };
            await context.WorkoutLogs.AddAsync(workoutLog);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Successfully verified and seeded master achievements and baseline data for test user in ProgressTrackingService.");
    }
}
