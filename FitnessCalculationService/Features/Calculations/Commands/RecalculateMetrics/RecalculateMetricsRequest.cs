namespace FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;

public record RecalculateMetricsRequest(double? NewWeight = null, string? Reason = null);
