using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;
using Xunit;

namespace FitnessCalculationService.Tests.Validators;

public class RecalculateMetricsOrchestratorValidatorTests
{
    private readonly RecalculateMetricsOrchestratorValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new RecalculateMetricsOrchestrator(
            UserId: Guid.NewGuid(),
            NewWeight: 74.0,
            Reason: "check-in",
            TriggeredBy: "user"
        );

        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(25.0)]
    [InlineData(350.0)]
    public void Validate_InvalidNewWeight_FailsValidation(double newWeight)
    {
        var command = new RecalculateMetricsOrchestrator(
            UserId: Guid.NewGuid(),
            NewWeight: newWeight,
            Reason: "check-in",
            TriggeredBy: "user"
        );

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValInvalidWeight);
    }
}
