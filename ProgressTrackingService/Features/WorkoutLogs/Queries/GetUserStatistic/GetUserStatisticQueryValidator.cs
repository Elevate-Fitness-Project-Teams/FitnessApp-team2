using FluentValidation;

namespace ProgressTrackingService.Features.WorkoutLogs.Queries.GetUserStatistic;

public class GetUserStatisticQueryValidator : AbstractValidator<GetUserStatisticQuery>
{
    public GetUserStatisticQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}
