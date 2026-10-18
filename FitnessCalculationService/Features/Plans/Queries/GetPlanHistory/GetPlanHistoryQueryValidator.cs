using FitnessCalculationService.Common.Errors;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanHistory;

public class GetPlanHistoryQueryValidator : AbstractValidator<GetPlanHistoryQuery>
{
    public GetPlanHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("UserId is required.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
