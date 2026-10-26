using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Errors;

namespace ProgressTrackingService.Common.Domain;

public record DateRange(DateTime From, DateTime To);

public record PeriodResolutionResult(DateRange CurrentRange, DateRange? PreviousRange, string Period);

public static class PeriodResolver
{
    private static readonly HashSet<string> ValidPeriods = new(StringComparer.OrdinalIgnoreCase)
    {
        "week", "month", "year", "all"
    };

    public static Result<PeriodResolutionResult> Resolve(
        string? period,
        DateTime? startDate,
        DateTime? endDate,
        DateTime nowUtc)
    {
        var today = nowUtc.Date;

        // 1. Custom date bounds take precedence
        if (startDate.HasValue || endDate.HasValue)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return Result.Failure<PeriodResolutionResult>(
                    Error.Validation(ProgressErrors.ValInvalidDate, "Both startDate and endDate must be provided for custom date ranges."));
            }

            var start = startDate.Value.Date;
            var end = endDate.Value.Date;

            if (start > end)
            {
                return Result.Failure<PeriodResolutionResult>(
                    Error.Validation(ProgressErrors.ValInvalidDate, "startDate cannot be after endDate."));
            }

            var daysSpan = (end - start).TotalDays + 1;
            if (daysSpan > 730) // Maximum 2 years
            {
                return Result.Failure<PeriodResolutionResult>(
                    Error.Validation(ProgressErrors.ValInvalidPeriod, "Date range cannot exceed 2 years."));
            }

            var prevStart = start.AddDays(-daysSpan);
            var prevEnd = start.AddDays(-1);

            return Result.Success(new PeriodResolutionResult(
                new DateRange(start, end),
                new DateRange(prevStart, prevEnd),
                "custom"));
        }

        // 2. Preset period fallback (default: "week")
        var p = string.IsNullOrWhiteSpace(period) ? "week" : period.Trim().ToLowerInvariant();
        if (!ValidPeriods.Contains(p))
        {
            return Result.Failure<PeriodResolutionResult>(
                Error.Validation(ProgressErrors.ValInvalidPeriod, $"Invalid period '{period}'. Allowed values: week, month, year, all."));
        }

        switch (p)
        {
            case "week":
            {
                var curStart = today.AddDays(-6);
                var curEnd = today;
                var prevStart = curStart.AddDays(-7);
                var prevEnd = curStart.AddDays(-1);
                return Result.Success(new PeriodResolutionResult(
                    new DateRange(curStart, curEnd),
                    new DateRange(prevStart, prevEnd),
                    "week"));
            }

            case "month":
            {
                var curStart = today.AddDays(-29);
                var curEnd = today;
                var prevStart = curStart.AddDays(-30);
                var prevEnd = curStart.AddDays(-1);
                return Result.Success(new PeriodResolutionResult(
                    new DateRange(curStart, curEnd),
                    new DateRange(prevStart, prevEnd),
                    "month"));
            }

            case "year":
            {
                var curStart = today.AddDays(-364);
                var curEnd = today;
                var prevStart = curStart.AddDays(-365);
                var prevEnd = curStart.AddDays(-1);
                return Result.Success(new PeriodResolutionResult(
                    new DateRange(curStart, curEnd),
                    new DateRange(prevStart, prevEnd),
                    "year"));
            }

            case "all":
            {
                var curStart = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var curEnd = today;
                return Result.Success(new PeriodResolutionResult(
                    new DateRange(curStart, curEnd),
                    null,
                    "all"));
            }

            default:
                return Result.Failure<PeriodResolutionResult>(
                    Error.Validation(ProgressErrors.ValInvalidPeriod, "Invalid period."));
        }
    }
}
