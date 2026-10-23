using System.ComponentModel.DataAnnotations;

namespace ProgressTrackingService.Models;

public class UserStatistic
{
    [Key]
    public Guid UserId { get; set; }
    public int TotalWorkouts { get; set; }
    public int TotalCaloriesBurned { get; set; }
    public int TotalMinutes { get; set; }
    public double InitialWeight { get; set; }
    public double CurrentWeight { get; set; }
    public double TotalWeightLost { get; set; }
    public DateTime UpdatedAt { get; set; }
}
