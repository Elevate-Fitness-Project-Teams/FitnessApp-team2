namespace FitnessCalculationService.Features.FitnessStats.Queries.GetFitnessStats;

public record GetFitnessStatsResponse(
    string UserId, 
    double Weight, 
    double Height, 
    int Age, 
    string Gender, 
    string Goal, 
    string ActivityLevel, 
    DateTime RecordedAt);
