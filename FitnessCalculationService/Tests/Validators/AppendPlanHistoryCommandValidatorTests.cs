using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Features.Plans.Commands;
using Xunit;

namespace FitnessCalculationService.Tests.Validators;

public class AppendPlanHistoryCommandValidatorTests
{
    private readonly AppendPlanHistoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new AppendPlanHistoryCommand(
            UserId: Guid.NewGuid(),
            PlanId: "LW-N",
            PreviousPlanId: null,
            Goal: FitnessGoal.LoseWeight,
            Status: CalorieStatus.Normal,
            PreviousStatus: null,
            Bmr: 1728.75,
            Tdee: 2679.56,
            CalorieTarget: 2179.56,
            AssignedPlanId: Guid.NewGuid(),
            TriggeredBy: "user",
            Reason: "initial_assignment"
        );

        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyPlanId_FailsValidation()
    {
        var command = new AppendPlanHistoryCommand(
            UserId: Guid.NewGuid(),
            PlanId: "",
            PreviousPlanId: null,
            Goal: FitnessGoal.LoseWeight,
            Status: CalorieStatus.Normal,
            PreviousStatus: null,
            Bmr: 1728.75,
            Tdee: 2679.56,
            CalorieTarget: 2179.56,
            AssignedPlanId: Guid.NewGuid(),
            TriggeredBy: "user",
            Reason: "initial_assignment"
        );

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValRequiredField);
    }

    [Fact]
    public void Validate_ZeroCalorieTarget_FailsValidation()
    {
        var command = new AppendPlanHistoryCommand(
            UserId: Guid.NewGuid(),
            PlanId: "LW-N",
            PreviousPlanId: null,
            Goal: FitnessGoal.LoseWeight,
            Status: CalorieStatus.Normal,
            PreviousStatus: null,
            Bmr: 1728.75,
            Tdee: 2679.56,
            CalorieTarget: 0,
            AssignedPlanId: Guid.NewGuid(),
            TriggeredBy: "user",
            Reason: "initial_assignment"
        );

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.FceInvalidCalculation);
    }
}
