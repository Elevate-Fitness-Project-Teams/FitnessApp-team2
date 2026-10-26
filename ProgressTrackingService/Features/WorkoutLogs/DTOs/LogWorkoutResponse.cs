using ProgressTrackingService.Features.Achievements.DTOs;

namespace ProgressTrackingService.Features.WorkoutLogs.DTOs;

public record LogWorkoutResponse(
    Guid LogId,
    bool StreakUpdated,
    int CurrentStreak,
    List<EarnedAchievementDto> NewAchievements
);
