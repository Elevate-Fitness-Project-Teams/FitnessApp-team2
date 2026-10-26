namespace ProgressTrackingService.Features.Achievements.DTOs;

public class UserAchievementDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string CriteriaType { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public bool Earned { get; set; }
    public DateTime? EarnedAt { get; set; }
}
