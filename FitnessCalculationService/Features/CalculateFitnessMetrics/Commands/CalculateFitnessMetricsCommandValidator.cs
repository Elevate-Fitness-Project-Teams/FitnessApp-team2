using FluentValidation;

namespace FitnessCalculationService.Features.CalculateFitnessMetrics.Commands
{
    public class CalculateFitnessMetricsCommandValidator : AbstractValidator<CalculateFitnessMetricsOrchstrator>
    {
        public CalculateFitnessMetricsCommandValidator()
        {
            RuleFor(v => v.UserId)
                .NotEmpty().WithMessage("UserId is required.");
        }
    }
}
