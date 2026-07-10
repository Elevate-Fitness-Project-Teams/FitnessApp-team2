namespace FitnessCalculationService.Domain.Entities;

public class UserPlanHistory
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}
