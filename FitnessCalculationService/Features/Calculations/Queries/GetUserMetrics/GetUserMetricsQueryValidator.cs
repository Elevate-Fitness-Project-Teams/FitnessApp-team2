using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;

public class GetUserMetricsQueryValidator : AbstractValidator<GetUserMetricsQuery>
{
    public GetUserMetricsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.")
            .Must(userId => Guid.TryParse(userId, out _))
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId must be a valid GUID format.");
    }
}
