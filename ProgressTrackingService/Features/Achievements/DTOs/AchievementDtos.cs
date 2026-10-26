namespace ProgressTrackingService.Features.Achievements.DTOs;

public record AchievementRuleDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string IconUrl,
    string CriteriaType,
    double Threshold
);

public record AchievementProgressSnapshotDto(
    int TotalWorkouts,
    int CurrentStreak,
    int TotalCaloriesBurned,
    double TotalWeightLost
);

public record EarnedAchievementDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string IconUrl,
    DateTime EarnedAt
);
