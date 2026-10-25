using MediatR;
using Microsoft.EntityFrameworkCore;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Biometrics.Commands.UpsertUserBiometrics;

public record UpsertUserBiometricsCommand(Guid UserId, double HeightCm, DateTime UpdatedAt) : IRequest<Result>;

public class UpsertUserBiometricsHandler : IRequestHandler<UpsertUserBiometricsCommand, Result>
{
    private readonly IGeneralRepo<UserBiometrics> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpsertUserBiometricsHandler> _logger;

    public UpsertUserBiometricsHandler(
        IGeneralRepo<UserBiometrics> repo,
        IUnitOfWork unitOfWork,
        ILogger<UpsertUserBiometricsHandler> logger)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpsertUserBiometricsCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var updatedRows = await _repo.Find(b => b.UserId == request.UserId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.HeightCm, request.HeightCm)
                    .SetProperty(b => b.UpdatedAt, request.UpdatedAt),
                    cancellationToken);

            if (updatedRows == 0)
            {
                var biometrics = new UserBiometrics
                {
                    UserId = request.UserId,
                    HeightCm = request.HeightCm,
                    UpdatedAt = request.UpdatedAt
                };

                await _repo.AddAsync(biometrics, cancellationToken);
            }

            _logger.LogInformation("Upserted user biometrics for user {UserId} with height {HeightCm} cm.",
                request.UserId, request.HeightCm);

            return Result.Success();
        }, cancellationToken);
    }
}
