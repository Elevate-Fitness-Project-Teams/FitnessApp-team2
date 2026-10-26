using ProgressTrackingService.Features.Achievements.DTOs;

namespace ProgressTrackingService.Common.Domain;

public static class AchievementRuleEvaluator
{
    public static IReadOnlyList<AchievementRuleDto> Evaluate(
        AchievementProgressSnapshotDto snapshot,
        IReadOnlyList<AchievementRuleDto> catalog,
        IReadOnlySet<Guid> alreadyEarnedAchievementIds)
    {
        var newlyEarned = new List<AchievementRuleDto>();

        foreach (var rule in catalog)
        {
            if (alreadyEarnedAchievementIds.Contains(rule.Id))
            {
                continue;
            }

            var currentValue = rule.CriteriaType switch
            {
                "TotalWorkouts" => snapshot.TotalWorkouts,
                "CurrentStreak" => snapshot.CurrentStreak,
                "TotalCaloriesBurned" => snapshot.TotalCaloriesBurned,
                "TotalWeightLost" => snapshot.TotalWeightLost,
                _ => 0.0
            };

            if (currentValue >= rule.Threshold)
            {
                newlyEarned.Add(rule);
            }
        }

        return newlyEarned;
    }
}
