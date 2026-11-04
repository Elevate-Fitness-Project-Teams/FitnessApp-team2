using ProgressTrackingService.Features.Streaks.Queries.GetStreak;
using ProgressTrackingService.Features.WorkoutLogs.Queries.GetAvailableAchievements;
using Xunit;

namespace ProgressTrackingService.Tests.Validators;

public class QueryValidatorsTests
{
    [Fact]
    public void GetStreakQueryValidator_WhenUserIdIsEmpty_HasValidationError()
    {
        var validator = new GetStreakQueryValidator();
        var query = new GetStreakQuery(Guid.Empty);

        var result = validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetStreakQuery.UserId));
    }

    [Fact]
    public void GetStreakQueryValidator_WhenUserIdIsValid_PassesValidation()
    {
        var validator = new GetStreakQueryValidator();
        var query = new GetStreakQuery(Guid.NewGuid());

        var result = validator.Validate(query);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public void GetAvailableAchievementsQueryValidator_WhenPagingInvalid_HasValidationErrors(int pageNumber, int pageSize)
    {
        var validator = new GetAvailableAchievementsQueryValidator();
        var query = new GetAvailableAchievementsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = validator.Validate(query);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GetAvailableAchievementsQueryValidator_WhenPagingValid_PassesValidation()
    {
        var validator = new GetAvailableAchievementsQueryValidator();
        var query = new GetAvailableAchievementsQuery
        {
            PageNumber = 1,
            PageSize = 20
        };

        var result = validator.Validate(query);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
