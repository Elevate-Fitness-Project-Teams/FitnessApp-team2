using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands;
using FitnessCalculationService.Features.SubmitWeightGoalActivity.Validators;
using Xunit;

namespace FitnessCalculationService.Tests.Validators;

public class SubmitFitnessStatsCommandValidatorTests
{
    private readonly SubmitFitnessStatsCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new SubmitFitnessStatsCommand(
            UserId: Guid.NewGuid(),
            Weight: 75.5,
            Height: 175.0,
            Age: 25,
            Gender: Gender.Male,
            Goal: FitnessGoal.LoseWeight,
            ActivityLevel: ActivityLevel.Intermediate,
            Source: "user"
        );

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUserId_FailsWithErrorCode()
    {
        var command = new SubmitFitnessStatsCommand(
            UserId: Guid.Empty,
            Weight: 75.5,
            Height: 175.0,
            Age: 25,
            Gender: Gender.Male,
            Goal: FitnessGoal.LoseWeight,
            ActivityLevel: ActivityLevel.Intermediate,
            Source: "user"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValRequiredField);
    }

    [Theory]
    [InlineData(11)]
    [InlineData(101)]
    public void Validate_InvalidAge_FailsWithValInvalidAge(int age)
    {
        var command = new SubmitFitnessStatsCommand(
            UserId: Guid.NewGuid(),
            Weight: 75.5,
            Height: 175.0,
            Age: age,
            Gender: Gender.Male,
            Goal: FitnessGoal.LoseWeight,
            ActivityLevel: ActivityLevel.Intermediate,
            Source: "user"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValInvalidAge);
    }

    [Theory]
    [InlineData(25.0)]
    [InlineData(350.0)]
    public void Validate_InvalidWeight_FailsWithValInvalidWeight(double weight)
    {
        var command = new SubmitFitnessStatsCommand(
            UserId: Guid.NewGuid(),
            Weight: weight,
            Height: 175.0,
            Age: 25,
            Gender: Gender.Male,
            Goal: FitnessGoal.LoseWeight,
            ActivityLevel: ActivityLevel.Intermediate,
            Source: "user"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValInvalidWeight);
    }

    [Theory]
    [InlineData(80.0)]
    [InlineData(260.0)]
    public void Validate_InvalidHeight_FailsWithValInvalidHeight(double height)
    {
        var command = new SubmitFitnessStatsCommand(
            UserId: Guid.NewGuid(),
            Weight: 75.5,
            Height: height,
            Age: 25,
            Gender: Gender.Male,
            Goal: FitnessGoal.LoseWeight,
            ActivityLevel: ActivityLevel.Intermediate,
            Source: "user"
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ErrorCodes.ValInvalidHeight);
    }
}
