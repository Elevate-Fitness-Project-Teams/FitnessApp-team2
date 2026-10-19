using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Features.Plans.Commands;
using Xunit;

namespace FitnessCalculationService.Tests.Validators;

public class CreateAssignedPlanCommandValidatorTests
{
    private readonly CreateAssignedPlanCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateAssignedPlanCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUserId_FailsValidation()
    {
        var command = new CreateAssignedPlanCommand(Guid.Empty, Guid.NewGuid());
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValRequiredField);
    }

    [Fact]
    public void Validate_EmptyConfigId_FailsValidation()
    {
        var command = new CreateAssignedPlanCommand(Guid.NewGuid(), Guid.Empty);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValRequiredField);
    }
}
