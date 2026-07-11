using MediatR;
using ProgressTrackingService.Common;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Models;

namespace ProgressTrackingService.Features.Progress.Commands.LogWeight;

public class SaveWeightEntryCommand : IRequest<Result<Guid>>
{
    public Guid UserId { get; set; }
    public double Weight { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
}

public class SaveWeightEntryHandler : IRequestHandler<SaveWeightEntryCommand, Result<Guid>>
{
    private readonly IGeneralRepo<WeightHistory> _weightHistoryRepo;
    private readonly IUnitOfWork _unitOfWork;
	public SaveWeightEntryHandler(IGeneralRepo<WeightHistory> weightHistoryRepo, IUnitOfWork unitOfWork)
    {
        _weightHistoryRepo = weightHistoryRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(SaveWeightEntryCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
		{
			var weightHistory = new WeightHistory
			{
				Id = Guid.CreateVersion7(),
				UserId = request.UserId,
				Weight = request.Weight,
				Date = request.Date,
				Notes = request.Notes
			};

			await _weightHistoryRepo.AddAsync(weightHistory, cancellationToken);

			return Result<Guid>.Success(weightHistory.Id);
		},cancellationToken);
    }
}
