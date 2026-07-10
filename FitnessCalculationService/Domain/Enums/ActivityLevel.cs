namespace FitnessCalculationService.Domain.Enums;

public enum ActivityLevel
{
    Rookie,
    Beginner,
    Intermediate,
    Advance,
    TrueBeast
}

public static class ActivityLevelExtensions
{
    public static double ToFactor(this ActivityLevel level) => level switch
    {
        ActivityLevel.Rookie => 1.2,
        ActivityLevel.Beginner => 1.375,
        ActivityLevel.Intermediate => 1.55,
        ActivityLevel.Advance => 1.725,
        ActivityLevel.TrueBeast => 1.9,
        _ => 1.2
    };
}
