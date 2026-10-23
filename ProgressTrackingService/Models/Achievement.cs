namespace ProgressTrackingService.Models;

public class Achievement
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string CriteriaType { get; set; } = string.Empty;
    public double Threshold { get; set; }
}
