using ProgressTrackingService.Common.Domain;
using ProgressTrackingService.Features.Achievements.DTOs;
using Xunit;

namespace ProgressTrackingService.Tests.Domain;

public class AchievementRuleEvaluatorTests
{
    private readonly List<AchievementRuleDto> _sampleCatalog = new()
    {
        new(Guid.NewGuid(), "FIRST_WORKOUT", "First Step", "First workout", "", "TotalWorkouts", 1),
        new(Guid.NewGuid(), "WORKOUTS_10", "10 Workouts", "10 workouts", "", "TotalWorkouts", 10),
        new(Guid.NewGuid(), "STREAK_3", "3-Day Streak", "3 days", "", "CurrentStreak", 3),
        new(Guid.NewGuid(), "CALORIES_1K", "1,000 Calories", "Burned 1k calories", "", "TotalCaloriesBurned", 1000),
        new(Guid.NewGuid(), "WEIGHT_LOST_5", "5kg Lost", "Lost 5kg", "", "TotalWeightLost", 5.0)
    };

    [Fact]
    public void Evaluate_UserReachesThresholds_ReturnsQualifyingUnearnedAchievements()
    {
        // Arrange
        var snapshot = new AchievementProgressSnapshotDto(
            TotalWorkouts: 1,
            CurrentStreak: 3,
            TotalCaloriesBurned: 1200,
            TotalWeightLost: 2.0);

        var earnedIds = new HashSet<Guid>(); // None earned yet

        // Act
        var newlyEarned = AchievementRuleEvaluator.Evaluate(snapshot, _sampleCatalog, earnedIds);

        // Assert
        Assert.Equal(3, newlyEarned.Count);
        Assert.Contains(newlyEarned, a => a.Code == "FIRST_WORKOUT");
        Assert.Contains(newlyEarned, a => a.Code == "STREAK_3");
        Assert.Contains(newlyEarned, a => a.Code == "CALORIES_1K");
        Assert.DoesNotContain(newlyEarned, a => a.Code == "WORKOUTS_10");
        Assert.DoesNotContain(newlyEarned, a => a.Code == "WEIGHT_LOST_5");
    }

    [Fact]
    public void Evaluate_AlreadyEarnedAchievements_AreExcluded()
    {
        // Arrange
        var firstWorkoutId = _sampleCatalog.First(a => a.Code == "FIRST_WORKOUT").Id;
        var earnedIds = new HashSet<Guid> { firstWorkoutId };

        var snapshot = new AchievementProgressSnapshotDto(
            TotalWorkouts: 5,
            CurrentStreak: 1,
            TotalCaloriesBurned: 200,
            TotalWeightLost: 0.0);

        // Act
        var newlyEarned = AchievementRuleEvaluator.Evaluate(snapshot, _sampleCatalog, earnedIds);

        // Assert - FIRST_WORKOUT threshold is met (5 >= 1), but it was already earned so excluded
        Assert.Empty(newlyEarned);
    }

    [Fact]
    public void Evaluate_WeightLossThreshold_UnlocksBadge()
    {
        // Arrange
        var snapshot = new AchievementProgressSnapshotDto(
            TotalWorkouts: 20,
            CurrentStreak: 2,
            TotalCaloriesBurned: 5000,
            TotalWeightLost: 6.5);

        var earnedIds = new HashSet<Guid>();

        // Act
        var newlyEarned = AchievementRuleEvaluator.Evaluate(snapshot, _sampleCatalog, earnedIds);

        // Assert
        Assert.Contains(newlyEarned, a => a.Code == "WEIGHT_LOST_5");
    }

    [Fact]
    public void Evaluate_UnknownCriteriaType_IgnoredSafely()
    {
        // Arrange
        var customCatalog = new List<AchievementRuleDto>
        {
            new(Guid.NewGuid(), "UNKNOWN", "Unknown", "Unknown", "", "UnknownCriteria", 10)
        };
        var snapshot = new AchievementProgressSnapshotDto(100, 100, 100, 100);

        // Act
        var newlyEarned = AchievementRuleEvaluator.Evaluate(snapshot, customCatalog, new HashSet<Guid>());

        // Assert
        Assert.Empty(newlyEarned);
    }
}
