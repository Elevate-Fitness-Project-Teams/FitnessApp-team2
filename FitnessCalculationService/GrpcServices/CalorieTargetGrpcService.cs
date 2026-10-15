using FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;
using FitnessCalculationService.Grpc;
using Grpc.Core;
using MediatR;

namespace FitnessCalculationService.GrpcServices;

public class CalorieTargetGrpcService : CalorieTargetService.CalorieTargetServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CalorieTargetGrpcService> _logger;

    public CalorieTargetGrpcService(IMediator mediator, ILogger<CalorieTargetGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<UserMetricsResponse> GetUserMetrics(UserMetricsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetUserMetrics called for User {UserId}", request.UserId);

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required."));
        }

        try
        {
            var result = await _mediator.Send(new GetUserMetricsQuery(request.UserId), context.CancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                return new UserMetricsResponse
                {
                    CalorieTarget = (int)Math.Round(result.Value.CalorieTarget),
                    Status = result.Value.Status,
                    IsCalculated = true
                };
            }

            return new UserMetricsResponse
            {
                CalorieTarget = 0,
                Status = string.Empty,
                IsCalculated = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing gRPC GetUserMetrics for User {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving user metrics"));
        }
    }
}
