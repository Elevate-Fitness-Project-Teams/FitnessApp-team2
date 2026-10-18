using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Domain.Enums;
using FluentValidation;

namespace FitnessCalculationService.Features.Plans.Queries.GetPlanConfigs;

public class GetPlanConfigsQueryValidator : AbstractValidator<GetPlanConfigsQuery>
{
    public GetPlanConfigsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode(ErrorCodes.ValRequiredField)
            .WithMessage("PageSize must be between 1 and 100.");

        When(x => !string.IsNullOrWhiteSpace(x.Goal), () =>
        {
            RuleFor(x => x.Goal)
                .Must(goal => Enum.TryParse<FitnessGoal>(goal!.Replace(" ", ""), true, out _))
                .WithErrorCode(ErrorCodes.ValInvalidGoal)
                .WithMessage("Invalid fitness goal filter specified.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Status), () =>
        {
            RuleFor(x => x.Status)
                .Must(status => Enum.TryParse<CalorieStatus>(status, true, out _))
                .WithErrorCode("VAL_INVALID_STATUS")
                .WithMessage("Invalid calorie status filter specified. Expected: Weak, Normal, Hard.");
        });
    }
}
