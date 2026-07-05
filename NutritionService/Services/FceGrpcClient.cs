using Grpc.Core;
using NutritionService.Grpc;
using NutritionService.Services.Dtos;

namespace NutritionService.Services
{
    public class FceGrpcClient : IFceGrpcClient
    {
        private readonly CalorieTargetService.CalorieTargetServiceClient _client;
        private readonly ILogger<FceGrpcClient> _logger;

        public FceGrpcClient(
            CalorieTargetService.CalorieTargetServiceClient client,
            ILogger<FceGrpcClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<UserCalorieTargetDto?> GetUserMetricsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var request = new UserMetricsRequest
            {
                UserId = userId.ToString()
            };

            try
            {

                var response = await _client.GetUserMetricsAsync(request, cancellationToken: cancellationToken);

                return new UserCalorieTargetDto
                {
                    CalorieTarget = response.CalorieTarget,
                    Status = response.Status,
                    IsCalculated = response.IsCalculated

                };
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                _logger.LogWarning("Metrics not found in FCE for User: {UserId}", userId);
                return null;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC call failed when reaching FCE Microservice for User: {UserId}", userId);
                throw;
            }
        }
    }
}