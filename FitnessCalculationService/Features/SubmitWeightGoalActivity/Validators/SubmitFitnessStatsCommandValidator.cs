using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands;
using FluentValidation;

namespace FitnessCalculationService.Features.SubmitWeightGoalActivity.Validators
{
    public class SubmitFitnessStatsCommandValidator : AbstractValidator<SubmitFitnessStatsCommand>
    {
        public SubmitFitnessStatsCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.ValRequiredField)
                .WithMessage("UserId is required.");

            RuleFor(x => x.Age)
                .InclusiveBetween(12, 100)
                .WithErrorCode(ErrorCodes.ValInvalidAge)
                .WithMessage("Age must be between 12 and 100.");

            RuleFor(x => x.Weight)
                .InclusiveBetween(30.0, 300.0)
                .WithErrorCode(ErrorCodes.ValInvalidWeight)
                .WithMessage("Weight must be between 30 and 300 kg.");

            RuleFor(x => x.Height)
                .InclusiveBetween(100.0, 250.0)
                .WithErrorCode(ErrorCodes.ValInvalidHeight)
                .WithMessage("Height must be between 100 and 250 cm.");

            RuleFor(x => x.Gender)
                .IsInEnum()
                .WithErrorCode(ErrorCodes.ValInvalidGender)
                .WithMessage("Invalid gender specified.");

            RuleFor(x => x.Goal)
                .IsInEnum()
                .WithErrorCode(ErrorCodes.ValInvalidGoal)
                .WithMessage("Invalid fitness goal specified.");

            RuleFor(x => x.ActivityLevel)
                .IsInEnum()
                .WithErrorCode(ErrorCodes.ValInvalidActivity)
                .WithMessage("Invalid activity level specified.");
        }
    }
}
