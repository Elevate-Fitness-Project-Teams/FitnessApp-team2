using ProgressTrackingService.Features.Progress.Queries.GetWeightHistory;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;

public class ViewUserProgressResponse
{
    public UserStatisticDto Statistics { get; set; } = new();
    public List<WorkoutLogDto> WorkoutLogs { get; set; } = new();
    public List<WeightHistoryDto> WeightHistory { get; set; } = new();
}

public class UserStatisticDto
{
    public int TotalWorkouts { get; set; }
    public int TotalCaloriesBurned { get; set; }
    public int TotalMinutes { get; set; }
    public double CurrentWeight { get; set; }
    public double TotalWeightLost { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WorkoutLogDto
{
    public Guid Id { get; set; }
    public Guid WorkoutId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Notes { get; set; }
    public DateTime CompletedAt { get; set; }
}

