using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Domain.Entities;

public class FitnessPlanConfig
{
    public Guid Id { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public FitnessGoal Goal { get; set; }
    public CalorieStatus Status { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<UserAssignedPlan> AssignedUsers { get; set; } = new List<UserAssignedPlan>();
}
