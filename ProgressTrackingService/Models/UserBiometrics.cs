using System.ComponentModel.DataAnnotations;

namespace ProgressTrackingService.Models;

public class UserBiometrics
{
    [Key]
    public Guid UserId { get; set; }
    public double HeightCm { get; set; }
    public DateTime UpdatedAt { get; set; }
}
