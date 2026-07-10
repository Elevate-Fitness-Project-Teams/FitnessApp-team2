using System.ComponentModel.DataAnnotations;

namespace ProgressTrackingService.Models;

public class WorkoutSession
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid WorkoutId { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public DateTime StartedAt { get; set; }
}
