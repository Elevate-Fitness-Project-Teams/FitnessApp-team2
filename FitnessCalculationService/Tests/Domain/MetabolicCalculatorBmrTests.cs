using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Domain.Services;
using Xunit;

namespace FitnessCalculationService.Tests.Domain;

public class MetabolicCalculatorBmrTests
{
    private readonly MetabolicCalculator _calculator = new();

    [Fact]
    public void CalculateBmr_Male_CalculatesCorrectly()
    {
        // Arrange
        // Male: (10 * 75.5) + (6.25 * 175) - (5 * 25) + 5 = 755 + 1093.75 - 125 + 5 = 1728.75
        double weight = 75.5;
        double height = 175.0;
        int age = 25;

        // Act
        var bmr = _calculator.CalculateBmr(weight, height, age, Gender.Male);

        // Assert
        Assert.Equal(1728.75, bmr, 2);
    }

    [Fact]
    public void CalculateBmr_Female_CalculatesCorrectly()
    {
        // Arrange
        // Female: (10 * 60) + (6.25 * 165) - (5 * 30) - 161 = 600 + 1031.25 - 150 - 161 = 1320.25
        double weight = 60.0;
        double height = 165.0;
        int age = 30;

        // Act
        var bmr = _calculator.CalculateBmr(weight, height, age, Gender.Female);

        // Assert
        Assert.Equal(1320.25, bmr, 2);
    }

    [Fact]
    public void CalculateBmr_UnsupportedGender_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.CalculateBmr(70, 170, 25, (Gender)999));
    }
}
