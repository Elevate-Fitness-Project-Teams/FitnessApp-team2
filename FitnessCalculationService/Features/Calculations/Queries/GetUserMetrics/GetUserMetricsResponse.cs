namespace FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;

public record GetUserMetricsResponse(
    string UserId, 
    double Bmr, 
    double Tdee, 
    double CalorieTarget, 
    DateTime CalculatedAt);
