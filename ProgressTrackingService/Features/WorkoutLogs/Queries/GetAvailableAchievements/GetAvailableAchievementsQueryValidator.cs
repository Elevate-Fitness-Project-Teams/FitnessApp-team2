using FluentValidation;

namespace ProgressTrackingService.Features.WorkoutLogs.Queries.GetAvailableAchievements;

public class GetAvailableAchievementsQueryValidator : AbstractValidator<GetAvailableAchievementsQuery>
{
    public GetAvailableAchievementsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}
