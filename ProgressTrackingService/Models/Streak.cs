using System.ComponentModel.DataAnnotations;

namespace ProgressTrackingService.Models;

public class Streak
{
    [Key]
    public Guid UserId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastWorkoutDate { get; set; }
}