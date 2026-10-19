using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Domain.Services;
using Xunit;

namespace FitnessCalculationService.Tests.Domain;

public class MetabolicCalculatorTargetTests
{
    private readonly MetabolicCalculator _calculator = new();

    [Theory]
    [InlineData(FitnessGoal.LoseWeight, -500.0, 2179.5625)]
    [InlineData(FitnessGoal.GetFitter, 0.0, 2679.5625)]
    [InlineData(FitnessGoal.GainWeight, 300.0, 2979.5625)]
    [InlineData(FitnessGoal.GainMoreFlexible, 150.0, 2829.5625)]
    [InlineData(FitnessGoal.LearnTheBasic, 0.0, 2679.5625)]
    public void CalculateCalorieTarget_AllFiveGoals_ApplyCorrectOffsets(FitnessGoal goal, double expectedOffset, double expectedTarget)
    {
        // Arrange
        double tdee = 2679.5625;

        // Act
        var target = _calculator.CalculateCalorieTarget(tdee, goal);

        // Assert
        Assert.Equal(tdee + expectedOffset, target, 4);
        Assert.Equal(expectedTarget, target, 4);
    }

    [Fact]
    public void CalculateCalorieTarget_InvalidGoal_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.CalculateCalorieTarget(2500, (FitnessGoal)999));
    }
}
