using NutritionService.Services.Dtos;

namespace NutritionService.Services;

public interface IFceHttpClient
{
    Task<UserCalorieTargetDto?> GetUserMetricsAsync(Guid userId, CancellationToken cancellationToken);
}
