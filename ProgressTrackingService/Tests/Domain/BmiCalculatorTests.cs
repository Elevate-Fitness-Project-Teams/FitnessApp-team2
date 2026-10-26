using ProgressTrackingService.Common.Domain;
using Xunit;

namespace ProgressTrackingService.Tests.Domain;

public class BmiCalculatorTests
{
    [Theory]
    [InlineData(70.0, 175.0, 22.9)]
    [InlineData(80.0, 180.0, 24.7)]
    [InlineData(95.0, 182.0, 28.7)]
    [InlineData(55.0, 160.0, 21.5)]
    public void Calculate_ValidWeightAndHeight_ReturnsExpectedRoundedBmi(double weightKg, double heightCm, double expectedBmi)
    {
        var result = BmiCalculator.Calculate(weightKg, heightCm);

        Assert.NotNull(result);
        Assert.Equal(expectedBmi, result.Value);
    }

    [Theory]
    [InlineData(0, 175.0)]
    [InlineData(-5.0, 180.0)]
    [InlineData(75.0, 0)]
    [InlineData(75.0, -170.0)]
    public void Calculate_InvalidValues_ReturnsNull(double weightKg, double heightCm)
    {
        var result = BmiCalculator.Calculate(weightKg, heightCm);

        Assert.Null(result);
    }

    [Fact]
    public void Calculate_NullHeight_ReturnsNull()
    {
        var result = BmiCalculator.Calculate(75.0, null);

        Assert.Null(result);
    }
}
