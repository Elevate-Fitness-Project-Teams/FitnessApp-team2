using FluentValidation;

namespace ProgressTrackingService.Features.WorkoutLogs.Queries.GetAvailableAchievements;

public class GetAvailableAchievementsQueryValidator : AbstractValidator<GetAvailableAchievementsQuery>
{
    public GetAvailableAchievementsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("PageSize must be greater than 0.");
    }
}
