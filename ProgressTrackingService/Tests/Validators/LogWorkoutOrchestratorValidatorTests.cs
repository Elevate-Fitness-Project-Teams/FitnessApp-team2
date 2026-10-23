using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;
using Xunit;

namespace ProgressTrackingService.Tests.Validators;

public class LogWorkoutOrchestratorValidatorTests
{
    private readonly LogWorkoutOrchestratorValidator _validator = new();

    [Fact]
    public void Validate_WhenPayloadIsValid_PassesValidation()
    {
        var command = new LogWorkoutOrchestrator(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "valid-session-123",
            DateTime.UtcNow.AddMinutes(-30),
            45,
            400,
            "Intermediate",
            "Great workout",
            4,
            new List<LogWorkoutExerciseDto>(),
            "Bearer token"
        );

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenUserIdEmpty_FailsWithValRequiredField()
    {
        var command = new LogWorkoutOrchestrator(
            Guid.Empty,
            Guid.NewGuid(),
            "valid-session-123",
            DateTime.UtcNow.AddMinutes(-30),
            45,
            400,
            "Intermediate",
            null,
            4,
            new List<LogWorkoutExerciseDto>(),
            ""
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ProgressErrors.ValRequiredField && e.PropertyName == "UserId");
    }

    [Fact]
    public void Validate_WhenDifficultyInvalid_FailsWithValInvalidDifficulty()
    {
        var command = new LogWorkoutOrchestrator(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "valid-session-123",
            DateTime.UtcNow.AddMinutes(-30),
            45,
            400,
            "SuperSaiyanMode", // Not a valid enum
            null,
            4,
            new List<LogWorkoutExerciseDto>(),
            ""
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ProgressErrors.ValInvalidDifficulty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_WhenRatingOutside1To5_FailsWithValInvalidRating(int rating)
    {
        var command = new LogWorkoutOrchestrator(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "valid-session-123",
            DateTime.UtcNow.AddMinutes(-30),
            45,
            400,
            "Beginner",
            null,
            rating,
            new List<LogWorkoutExerciseDto>(),
            ""
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ProgressErrors.ValInvalidRating);
    }

    [Fact]
    public void Validate_WhenCompletedAtInFuture_FailsWithValInvalidDate()
    {
        var command = new LogWorkoutOrchestrator(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "valid-session-123",
            DateTime.UtcNow.AddDays(1), // Future date
            45,
            400,
            "Beginner",
            null,
            4,
            new List<LogWorkoutExerciseDto>(),
            ""
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ProgressErrors.ValInvalidDate);
    }
}
