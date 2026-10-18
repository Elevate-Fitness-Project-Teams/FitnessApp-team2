using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Enums;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Commands;

public class AppendPlanHistoryCommandValidator : AbstractValidator<AppendPlanHistoryCommand>
{
    public AppendPlanHistoryCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");

        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("PlanId is required.");

        RuleFor(x => x.Goal)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.ValInvalidGoal)
            .WithMessage("Invalid fitness goal.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("Invalid calorie status.");

        When(x => x.PreviousStatus.HasValue, () =>
        {
            RuleFor(x => x.PreviousStatus!.Value)
                .IsInEnum()
                .WithErrorCode(ErrorCodes.ValRequiredField)
                .WithMessage("Invalid previous calorie status.");
        });

        RuleFor(x => x.Bmr)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.FceInvalidCalculation)
            .WithMessage("BMR must be greater than zero.");

        RuleFor(x => x.Tdee)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.FceInvalidCalculation)
            .WithMessage("TDEE must be greater than zero.");

        RuleFor(x => x.CalorieTarget)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.FceInvalidCalculation)
            .WithMessage("Calorie target must be greater than zero.");

        When(x => x.AssignedPlanId.HasValue, () =>
        {
            RuleFor(x => x.AssignedPlanId!.Value)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.ValRequiredField)
                .WithMessage("AssignedPlanId cannot be empty when provided.");
        });

        RuleFor(x => x.TriggeredBy)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("TriggeredBy is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("Reason is required.");
    }
}
