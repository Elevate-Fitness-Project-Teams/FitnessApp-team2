using FluentValidation;

namespace ProgressTrackingService.Features.Streaks.Queries.GetStreak;

public class GetStreakQueryValidator : AbstractValidator<GetStreakQuery>
{
    public GetStreakQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}
