namespace FitnessCalculationService.Domain.Entities;

public class UserAssignedPlan
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid FitnessPlanConfigId { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }

    public FitnessPlanConfig FitnessPlanConfig { get; set; } = null!;
}
