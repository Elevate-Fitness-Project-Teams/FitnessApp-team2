namespace FitnessCalculationService.Features.CalculateFitnessMetrics.Response
{
    public record CalculateFitnessMetricsResponse(double Bmr, double Tdee, double CalorieTarget, string Status);
}
