using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Domain.Services;

public class MetabolicCalculator : IMetabolicCalculator
{
    public double CalculateBmr(double weight, double height, int age, Gender gender)
    {
        // Mifflin-St Jeor Equation
        double bmr = (10 * weight) + (6.25 * height) - (5 * age);
        return gender == Gender.Male ? bmr + 5 : bmr - 161;
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
            FitnessGoal.GainWeight => tdee + 500,
            FitnessGoal.GainMoreFlexible => tdee,
            FitnessGoal.LearnTheBasic => tdee,
            _ => tdee
        };
    }

    public CalorieStatus ClassifyStatus(double calorieTarget)
    {
        if (calorieTarget < 1500) return CalorieStatus.Weak;
        if (calorieTarget > 2500) return CalorieStatus.Hard;
        return CalorieStatus.Normal;
    }
}
