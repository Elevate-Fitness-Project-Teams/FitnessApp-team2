using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Domain.Services;
using Xunit;

namespace FitnessCalculationService.Tests.Domain;

public class MetabolicCalculatorWorkedExampleTests
{
    private readonly MetabolicCalculator _calculator = new();

    [Fact]
    public void Calculate_WorkedExampleMale_ReproducesExactExpectedValues()
    {
        // Worked example from implementation plan Task 2.1:
        // Male, 75.5 kg, 175 cm, 25 y, Intermediate, Lose Weight
        // BMR: 1728.75, TDEE: 2679.56, Target: 2179.56, Status: Normal
        var result = _calculator.Calculate(
            weight: 75.5,
            height: 175.0,
            age: 25,
            gender: Gender.Male,
            activityLevel: ActivityLevel.Intermediate,
            goal: FitnessGoal.LoseWeight
        );

        Assert.NotNull(result);
        Assert.Equal(1728.75, result.Bmr);
        Assert.Equal(2679.56, result.Tdee);
        Assert.Equal(2179.56, result.CalorieTarget);
        Assert.Equal(CalorieStatus.Normal, result.Status);
    }

    [Fact]
    public void Calculate_FemaleGainWeight_CalculatesAccurately()
    {
        // Female, 60 kg, 165 cm, 30 y, Beginner (1.375), Gain Weight (+300)
        // BMR: 10*60 + 6.25*165 - 5*30 - 161 = 1320.25
        // TDEE: 1320.25 * 1.375 = 1815.34
        // Target: 1815.34 + 300 = 2115.34
        // Status: Normal (between 1801 and 2500)
        var result = _calculator.Calculate(
            weight: 60.0,
            height: 165.0,
            age: 30,
            gender: Gender.Female,
            activityLevel: ActivityLevel.Beginner,
            goal: FitnessGoal.GainWeight
        );

        Assert.NotNull(result);
        Assert.Equal(1320.25, result.Bmr);
        Assert.Equal(1815.34, result.Tdee);
        Assert.Equal(2115.34, result.CalorieTarget);
        Assert.Equal(CalorieStatus.Normal, result.Status);
    }

    [Fact]
    public void Calculate_NaNOrInfinity_ReturnsNull()
    {
        // Act
        var nanResult = _calculator.Calculate(
            weight: double.NaN,
            height: 175,
            age: 25,
            gender: Gender.Male,
            activityLevel: ActivityLevel.Intermediate,
            goal: FitnessGoal.LoseWeight
        );

        var infinityResult = _calculator.Calculate(
            weight: double.PositiveInfinity,
            height: 175,
            age: 25,
            gender: Gender.Male,
            activityLevel: ActivityLevel.Intermediate,
            goal: FitnessGoal.LoseWeight
        );

        // Assert
        Assert.Null(nanResult);
        Assert.Null(infinityResult);
    }
}
