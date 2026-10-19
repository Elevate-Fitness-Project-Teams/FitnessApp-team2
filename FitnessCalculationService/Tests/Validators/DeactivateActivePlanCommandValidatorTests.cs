using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Features.Plans.Commands;
using Xunit;

namespace FitnessCalculationService.Tests.Validators;

public class DeactivateActivePlanCommandValidatorTests
{
    private readonly DeactivateActivePlanCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new DeactivateActivePlanCommand(Guid.NewGuid());
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUserId_FailsValidation()
    {
        var command = new DeactivateActivePlanCommand(Guid.Empty);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValRequiredField);
    }
}
