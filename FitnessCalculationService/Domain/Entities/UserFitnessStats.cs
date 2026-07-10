using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Domain.Entities;

public class UserFitnessStats
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public FitnessGoal Goal { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    public DateTime RecordedAt { get; set; }
}
