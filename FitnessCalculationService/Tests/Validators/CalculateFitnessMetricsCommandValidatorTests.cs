using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Features.CalculateFitnessMetrics.Commands;
using Xunit;

namespace FitnessCalculationService.Tests.Validators;

public class CalculateFitnessMetricsCommandValidatorTests
{
    private readonly CalculateFitnessMetricsCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CalculateFitnessMetricsCommand(
            UserId: Guid.NewGuid(),
            Bmr: 1728.75,
            Tdee: 2679.56,
            CalorieTarget: 2179.56,
            Status: CalorieStatus.Normal
        );

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUserId_FailsValidation()
    {
        var command = new CalculateFitnessMetricsCommand(
            UserId: Guid.Empty,
            Bmr: 1728.75,
            Tdee: 2679.56,
            CalorieTarget: 2179.56,
            Status: CalorieStatus.Normal
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValRequiredField);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_ZeroOrNegativeBmr_FailsValidation(double bmr)
    {
        var command = new CalculateFitnessMetricsCommand(
            UserId: Guid.NewGuid(),
            Bmr: bmr,
            Tdee: 2679.56,
            CalorieTarget: 2179.56,
            Status: CalorieStatus.Normal
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.FceInvalidCalculation);
    }
}
