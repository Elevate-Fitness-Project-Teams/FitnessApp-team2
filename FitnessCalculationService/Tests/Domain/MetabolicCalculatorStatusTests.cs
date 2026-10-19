using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Domain.Services;
using Xunit;

namespace FitnessCalculationService.Tests.Domain;

public class MetabolicCalculatorStatusTests
{
    private readonly MetabolicCalculator _calculator = new();

    [Theory]
    [InlineData(1200.0, CalorieStatus.Weak)]
    [InlineData(1799.99, CalorieStatus.Weak)]
    [InlineData(1800.00, CalorieStatus.Weak)]
    [InlineData(1800.01, CalorieStatus.Normal)]
    [InlineData(2179.56, CalorieStatus.Normal)]
    [InlineData(2500.00, CalorieStatus.Normal)]
    [InlineData(2500.01, CalorieStatus.Hard)]
    [InlineData(3200.00, CalorieStatus.Hard)]
    public void ClassifyStatus_ThresholdBoundaries_ClassifyCorrectly(double calorieTarget, CalorieStatus expectedStatus)
    {
        // Act
        var status = _calculator.ClassifyStatus(calorieTarget);

        // Assert
        Assert.Equal(expectedStatus, status);
    }
}
