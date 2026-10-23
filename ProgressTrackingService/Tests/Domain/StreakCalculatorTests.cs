using ProgressTrackingService.Common.Domain;
using Xunit;

namespace ProgressTrackingService.Tests.Domain;

public class StreakCalculatorTests
{
    [Fact]
    public void Calculate_FirstWorkout_ReturnsStreakOfOne()
    {
        // Arrange
        var completedAt = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = StreakCalculator.Calculate(0, 0, null, completedAt);

        // Assert
        Assert.Equal(1, result.CurrentStreak);
        Assert.Equal(1, result.LongestStreak);
        Assert.True(result.StreakUpdated);
    }

    [Fact]
    public void Calculate_ConsecutiveDay_IncrementsStreakAndUpdatesLongest()
    {
        // Arrange
        var lastWorkout = new DateTime(2026, 10, 20, 18, 0, 0, DateTimeKind.Utc);
        var completedAt = new DateTime(2026, 10, 21, 9, 0, 0, DateTimeKind.Utc);

        // Act
        var result = StreakCalculator.Calculate(3, 3, lastWorkout, completedAt);

        // Assert
        Assert.Equal(4, result.CurrentStreak);
        Assert.Equal(4, result.LongestStreak);
        Assert.True(result.StreakUpdated);
    }

    [Fact]
    public void Calculate_SameDayWorkout_PreservesStreakWithoutIncrementing()
    {
        // Arrange
        var lastWorkout = new DateTime(2026, 10, 21, 8, 0, 0, DateTimeKind.Utc);
        var completedAt = new DateTime(2026, 10, 21, 19, 0, 0, DateTimeKind.Utc);

        // Act
        var result = StreakCalculator.Calculate(4, 5, lastWorkout, completedAt);

        // Assert
        Assert.Equal(4, result.CurrentStreak);
        Assert.Equal(5, result.LongestStreak);
        Assert.False(result.StreakUpdated);
    }

    [Fact]
    public void Calculate_MissedTwoDays_ResetsCurrentStreakToOnePreservingLongest()
    {
        // Arrange
        var lastWorkout = new DateTime(2026, 10, 18, 10, 0, 0, DateTimeKind.Utc);
        var completedAt = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Utc); // 3 days gap

        // Act
        var result = StreakCalculator.Calculate(7, 10, lastWorkout, completedAt);

        // Assert
        Assert.Equal(1, result.CurrentStreak);
        Assert.Equal(10, result.LongestStreak);
        Assert.True(result.StreakUpdated);
    }

    [Fact]
    public void Calculate_BackDatedWorkout_PreservesExistingStreak()
    {
        // Arrange
        var lastWorkout = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Utc);
        var backDated = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = StreakCalculator.Calculate(5, 5, lastWorkout, backDated);

        // Assert
        Assert.Equal(5, result.CurrentStreak);
        Assert.Equal(5, result.LongestStreak);
        Assert.False(result.StreakUpdated);
    }

    [Fact]
    public void EffectiveCurrentStreak_WhenMissedDay_DecaysToZero()
    {
        // Arrange
        var lastWorkout = new DateTime(2026, 10, 19, 10, 0, 0, DateTimeKind.Utc);
        var now = new DateTime(2026, 10, 21, 14, 0, 0, DateTimeKind.Utc); // 2 days since last workout

        // Act
        var effective = StreakCalculator.EffectiveCurrentStreak(5, lastWorkout, now);

        // Assert
        Assert.Equal(0, effective);
    }

    [Fact]
    public void EffectiveCurrentStreak_WhenWithinGracePeriod_MaintainsStreak()
    {
        // Arrange
        var lastWorkout = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);
        var now = new DateTime(2026, 10, 21, 14, 0, 0, DateTimeKind.Utc); // 1 day difference

        // Act
        var effective = StreakCalculator.EffectiveCurrentStreak(5, lastWorkout, now);

        // Assert
        Assert.Equal(5, effective);
    }
}
