using FitnessCalculationService.Domain.Enums;

namespace FitnessCalculationService.Domain.Services;

public interface IMetabolicCalculator
{
    double CalculateBmr(double weight, double height, int age, Gender gender);
    double CalculateTdee(double bmr, ActivityLevel activityLevel);
    double CalculateCalorieTarget(double tdee, FitnessGoal goal);
    CalorieStatus ClassifyStatus(double calorieTarget);
}
