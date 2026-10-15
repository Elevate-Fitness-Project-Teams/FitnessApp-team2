using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Domain.Services;

public record MetabolicResult(double Bmr, double Tdee, double CalorieTarget, CalorieStatus Status);

public interface IMetabolicCalculator
{
    double CalculateBmr(double weight, double height, int age, Gender gender);
    double CalculateTdee(double bmr, ActivityLevel activityLevel);
    double CalculateCalorieTarget(double tdee, FitnessGoal goal);
    CalorieStatus ClassifyStatus(double calorieTarget);
    MetabolicResult? Calculate(double weight, double height, int age, Gender gender, ActivityLevel activityLevel, FitnessGoal goal);
}
