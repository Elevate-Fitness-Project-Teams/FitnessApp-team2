using ProgressTrackingService.Common.Domain;
using ProgressTrackingService.Common.Errors;
using Xunit;

namespace ProgressTrackingService.Tests.Domain;

public class PeriodResolverTests
{
    [Fact]
    public void Resolve_WhenWeekPeriod_Calculates7DayCurrentAndPrevious()
    {
        var now = new DateTime(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc);
        var today = now.Date;

        var result = PeriodResolver.Resolve("week", null, null, now);

        Assert.True(result.IsSuccess);
        Assert.Equal("week", result.Value.Period);
        Assert.Equal(today.AddDays(-6), result.Value.CurrentRange.From);
        Assert.Equal(today, result.Value.CurrentRange.To);
        Assert.NotNull(result.Value.PreviousRange);
        Assert.Equal(today.AddDays(-13), result.Value.PreviousRange!.From);
        Assert.Equal(today.AddDays(-7), result.Value.PreviousRange!.To);
    }

    [Fact]
    public void Resolve_WhenMonthPeriod_Calculates30DayCurrentAndPrevious()
    {
        var now = new DateTime(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc);
        var today = now.Date;

        var result = PeriodResolver.Resolve("month", null, null, now);

        Assert.True(result.IsSuccess);
        Assert.Equal("month", result.Value.Period);
        Assert.Equal(today.AddDays(-29), result.Value.CurrentRange.From);
        Assert.Equal(today, result.Value.CurrentRange.To);
        Assert.NotNull(result.Value.PreviousRange);
        Assert.Equal(today.AddDays(-59), result.Value.PreviousRange!.From);
        Assert.Equal(today.AddDays(-30), result.Value.PreviousRange!.To);
    }

    [Fact]
    public void Resolve_WhenAllPeriod_HasNoPreviousRange()
    {
        var now = new DateTime(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc);

        var result = PeriodResolver.Resolve("all", null, null, now);

        Assert.True(result.IsSuccess);
        Assert.Equal("all", result.Value.Period);
        Assert.Null(result.Value.PreviousRange);
    }

    [Fact]
    public void Resolve_WhenCustomStartDateAfterEndDate_ReturnsValidationError()
    {
        var now = DateTime.UtcNow;
        var start = now.Date;
        var end = now.Date.AddDays(-5);

        var result = PeriodResolver.Resolve(null, start, end, now);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.ValInvalidDate, result.Error.Code);
    }

    [Fact]
    public void Resolve_WhenCustomSpanExceedsTwoYears_ReturnsValidationError()
    {
        var now = DateTime.UtcNow;
        var start = now.Date.AddDays(-800);
        var end = now.Date;

        var result = PeriodResolver.Resolve(null, start, end, now);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.ValInvalidPeriod, result.Error.Code);
    }

    [Fact]
    public void Resolve_WhenPeriodIsInvalidString_ReturnsValidationError()
    {
        var now = DateTime.UtcNow;

        var result = PeriodResolver.Resolve("biweekly", null, null, now);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProgressErrors.ValInvalidPeriod, result.Error.Code);
    }
}
