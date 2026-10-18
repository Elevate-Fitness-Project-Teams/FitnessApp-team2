using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.CalculateFitnessMetrics.Commands;

public class CalculateFitnessMetricsCommandValidator : AbstractValidator<CalculateFitnessMetricsCommand>
{
    public CalculateFitnessMetricsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");

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
            .WithMessage("CalorieTarget must be greater than zero.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("Invalid calorie status.");

        When(x => x.SourceStatsId.HasValue, () =>
        {
            RuleFor(x => x.SourceStatsId!.Value)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.ValRequiredField)
                .WithMessage("SourceStatsId cannot be empty when provided.");
        });
    }
}
