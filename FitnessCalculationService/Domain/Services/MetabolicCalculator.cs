using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Domain.Services;

public class MetabolicCalculator : IMetabolicCalculator
{
    public double CalculateBmr(double weight, double height, int age, Gender gender)
    {
        double bmr = (10 * weight) + (6.25 * height) - (5 * age);
        return gender switch
        {
            Gender.Male => bmr + 5,
            Gender.Female => bmr - 161,
            _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, $"Unsupported gender: {gender}")
        };
    }

    public double CalculateTdee(double bmr, ActivityLevel activityLevel)
    {
        return bmr * activityLevel.ToFactor();
    }

    public double CalculateCalorieTarget(double tdee, FitnessGoal goal)
    {
        return goal switch
        {
            FitnessGoal.LoseWeight => tdee - 500,
            FitnessGoal.GetFitter => tdee,
            FitnessGoal.GainWeight => tdee + 300,
            FitnessGoal.GainMoreFlexible => tdee + 150,
            FitnessGoal.LearnTheBasic => tdee,
            _ => throw new ArgumentOutOfRangeException(nameof(goal), goal, $"Unsupported fitness goal: {goal}")
        };
    }

    public CalorieStatus ClassifyStatus(double calorieTarget)
    {
        if (calorieTarget <= 1800) return CalorieStatus.Weak;
        if (calorieTarget <= 2500) return CalorieStatus.Normal;
        return CalorieStatus.Hard;
    }

    public MetabolicResult? Calculate(double weight, double height, int age, Gender gender, ActivityLevel activityLevel, FitnessGoal goal)
    {
        var rawBmr = CalculateBmr(weight, height, age, gender);
        var rawTdee = CalculateTdee(rawBmr, activityLevel);
        var rawTarget = CalculateCalorieTarget(rawTdee, goal);

        if (!double.IsFinite(rawBmr) || !double.IsFinite(rawTdee) || !double.IsFinite(rawTarget))
        {
            return null;
        }

        var status = ClassifyStatus(rawTarget);

        var roundedBmr = Math.Round(rawBmr, 2, MidpointRounding.AwayFromZero);
        var roundedTdee = Math.Round(rawTdee, 2, MidpointRounding.AwayFromZero);
        var roundedTarget = Math.Round(rawTarget, 2, MidpointRounding.AwayFromZero);

        return new MetabolicResult(roundedBmr, roundedTdee, roundedTarget, status);
    }
}
