using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Domain.Services;
using Xunit;

namespace FitnessCalculationService.Tests.Domain;

public class MetabolicCalculatorTdeeTests
{
    private readonly MetabolicCalculator _calculator = new();

    [Theory]
    [InlineData(ActivityLevel.Rookie, 1.2, 2074.5)]
    [InlineData(ActivityLevel.Beginner, 1.375, 2377.03125)]
    [InlineData(ActivityLevel.Intermediate, 1.55, 2679.5625)]
    [InlineData(ActivityLevel.Advance, 1.725, 2982.09375)]
    [InlineData(ActivityLevel.TrueBeast, 1.9, 3284.625)]
    public void CalculateTdee_AllFiveActivityLevels_ApplyCorrectFactors(ActivityLevel level, double expectedFactor, double expectedTdee)
    {
        // Arrange
        double bmr = 1728.75;

        // Act
        var tdee = _calculator.CalculateTdee(bmr, level);

        // Assert
        Assert.Equal(expectedFactor, level.ToFactor());
        Assert.Equal(expectedTdee, tdee, 4);
    }

    [Fact]
    public void CalculateTdee_InvalidActivityLevel_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.CalculateTdee(1500, (ActivityLevel)999));
    }
}
