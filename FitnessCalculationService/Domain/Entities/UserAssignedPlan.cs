namespace FitnessCalculationService.Domain.Entities;

public class UserAssignedPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FitnessPlanConfigId { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }

    public FitnessPlanConfig FitnessPlanConfig { get; set; } = null!;
}
