
using NutritionService.Services.Dtos;

namespace NutritionService.Services
{
    public interface IFceGrpcClient
    {
        Task<UserCalorieTargetDto?> GetUserMetricsAsync(Guid userId, CancellationToken cancellationToken);
    }
}