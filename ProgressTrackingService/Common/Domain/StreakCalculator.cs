namespace ProgressTrackingService.Common.Domain;

public record StreakCalculationResult(int CurrentStreak, int LongestStreak, bool StreakUpdated);

public static class StreakCalculator
{
    public static StreakCalculationResult Calculate(int currentStreak, int longestStreak, DateTime? lastWorkoutDate, DateTime completedAt)
    {
        if (!lastWorkoutDate.HasValue)
        {
            return new StreakCalculationResult(1, Math.Max(1, longestStreak), true);
        }

        var completedDate = completedAt.Date;
        var lastDate = lastWorkoutDate.Value.Date;

        if (completedDate < lastDate)
        {
            // Back-dated workout earlier than the recorded last workout date: streak does not increment
            return new StreakCalculationResult(currentStreak, longestStreak, false);
        }

        var daysDiff = (completedDate - lastDate).TotalDays;

        if (daysDiff == 0)
        {
            // Completed another workout on the same calendar day: streak preserved, not incremented
            return new StreakCalculationResult(currentStreak, longestStreak, false);
        }

        if (daysDiff == 1)
        {
            // Consecutive day: increment current streak
            var newCurrent = currentStreak + 1;
            var newLongest = Math.Max(newCurrent, longestStreak);
            return new StreakCalculationResult(newCurrent, newLongest, true);
        }

        // Gap of 2 or more days: streak resets to 1
        return new StreakCalculationResult(1, longestStreak, true);
    }

    /// <summary>
    /// Computes the effective current streak at read-time. If more than 1 calendar day has elapsed
    /// since the last workout date, the active streak has lapsed and is reported as 0.
    /// </summary>
    public static int EffectiveCurrentStreak(int currentStreak, DateTime? lastWorkoutDate, DateTime utcNow)
    {
        if (!lastWorkoutDate.HasValue || currentStreak <= 0)
        {
            return 0;
        }

        var daysSinceLastWorkout = (utcNow.Date - lastWorkoutDate.Value.Date).TotalDays;
        return daysSinceLastWorkout <= 1 ? currentStreak : 0;
    }
}
