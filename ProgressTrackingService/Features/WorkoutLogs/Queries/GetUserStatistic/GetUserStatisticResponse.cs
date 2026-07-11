namespace ProgressTrackingService.Features.WorkoutLogs.Queries.GetUserStatistic;

public class GetUserStatisticResponse
{
    public Guid UserId { get; set; }
    public int TotalWorkouts { get; set; }
    public int TotalCaloriesBurned { get; set; }
    public DateTime UpdatedAt { get; set; }
}
