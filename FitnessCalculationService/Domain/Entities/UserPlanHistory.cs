using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Domain.Entities;

public class UserPlanHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public string? PreviousPlanId { get; set; }
    public FitnessGoal Goal { get; set; }
    public CalorieStatus Status { get; set; }
    public CalorieStatus? PreviousStatus { get; set; }
    public double Bmr { get; set; }
    public double Tdee { get; set; }
    public double CalorieTarget { get; set; }
    public DateTime AssignedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string TriggeredBy { get; set; } = "user";
    public Guid? AssignedPlanId { get; set; }
}
