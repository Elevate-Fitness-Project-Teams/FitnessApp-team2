using ProgressTrackingService.Features.Achievements.DTOs;
using ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

namespace ProgressTrackingService.Features.Progress.Queries.GetProgressDashboard;

public class ProgressDashboardResponse
{
    public string Period { get; set; } = string.Empty;
    public DashboardDateRangeDto Range { get; set; } = new();
    public DashboardSummaryDto Summary { get; set; } = new();
    public DashboardComparisonDto Comparison { get; set; } = new();
    public DashboardStreakDto Streak { get; set; } = new();
    public List<WeightHistoryDto> WeightHistory { get; set; } = new();
    public List<WorkoutLogDto> WorkoutHistory { get; set; } = new();
    public List<UserAchievementDto> AchievementsEarned { get; set; } = new();
}

public class DashboardDateRangeDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public class DashboardSummaryDto
{
    public int Workouts { get; set; }
    public int CaloriesBurned { get; set; }
    public int Minutes { get; set; }
    public double AvgRating { get; set; }
    public double? WeightChange { get; set; }
}

public class DashboardComparisonDto
{
    public double? WorkoutsChangePct { get; set; }
    public double? CaloriesChangePct { get; set; }
    public double? MinutesChangePct { get; set; }
}

public class DashboardStreakDto
{
    public int Current { get; set; }
    public int Longest { get; set; }
}
