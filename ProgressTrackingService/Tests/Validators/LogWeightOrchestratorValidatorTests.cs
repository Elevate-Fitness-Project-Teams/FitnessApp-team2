using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Features.Progress.Orchestrators;
using Xunit;

namespace ProgressTrackingService.Tests.Validators;

public class LogWeightOrchestratorValidatorTests
{
    private readonly LogWeightOrchestratorValidator _validator = new();

    [Fact]
    public void Validate_WhenPayloadIsValid_PassesValidation()
    {
        var command = new LogWeightOrchestrator(
            Guid.NewGuid(),
            78.5,
            DateTime.UtcNow.AddHours(-1),
            "Morning weigh-in"
        );

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(39.9)]
    [InlineData(200.1)]
    [InlineData(0)]
    public void Validate_WhenWeightOutsideRange_FailsWithValInvalidWeight(double weight)
    {
        var command = new LogWeightOrchestrator(
            Guid.NewGuid(),
            weight,
            DateTime.UtcNow.AddHours(-1),
            null
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ProgressErrors.ValInvalidWeight);
    }

    [Fact]
    public void Validate_WhenDateInFuture_FailsWithValInvalidDate()
    {
        var command = new LogWeightOrchestrator(
            Guid.NewGuid(),
            75.0,
            DateTime.UtcNow.AddDays(2),
            null
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ProgressErrors.ValInvalidDate);
    }

    [Fact]
    public void Validate_WhenDateBeforeYear2000_FailsWithValInvalidDate()
    {
        var command = new LogWeightOrchestrator(
            Guid.NewGuid(),
            75.0,
            new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            null
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ProgressErrors.ValInvalidDate);
    }

    [Fact]
    public void Validate_WhenNotesExceed500Chars_FailsValidation()
    {
        var command = new LogWeightOrchestrator(
            Guid.NewGuid(),
            75.0,
            DateTime.UtcNow.AddHours(-1),
            new string('A', 501)
        );

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == ProgressErrors.ValRequiredField);
    }
}
