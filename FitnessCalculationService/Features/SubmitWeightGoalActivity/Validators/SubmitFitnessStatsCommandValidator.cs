using FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands;
using FluentValidation;

namespace FitnessCalculationService.Features.SubmitWeightGoalActivity.Validators
{
    public class SubmitFitnessStatsCommandValidator : AbstractValidator<SubmitFitnessStatsCommand>
    {
        public SubmitFitnessStatsCommandValidator()
        {
            
            RuleFor(x => x.Age)
                .InclusiveBetween(16, 100)
                .WithMessage("VAL_INVALID_AGE");

            RuleFor(x => x.Weight)
                .InclusiveBetween(40, 200)
                .WithMessage("VAL_INVALID_WEIGHT");

            RuleFor(x => x.Height)
                .InclusiveBetween(140, 220)
                .WithMessage("VAL_INVALID_HEIGHT");

            
            RuleFor(x => x.Gender)
                .IsInEnum()
                .WithMessage("VAL_INVALID_GENDER");

            RuleFor(x => x.Goal)
                .IsInEnum()
                .WithMessage("VAL_INVALID_GOAL");

            RuleFor(x => x.ActivityLevel)
                .IsInEnum()
                .WithMessage("VAL_INVALID_ACTIVITY");
        }
    }
}
