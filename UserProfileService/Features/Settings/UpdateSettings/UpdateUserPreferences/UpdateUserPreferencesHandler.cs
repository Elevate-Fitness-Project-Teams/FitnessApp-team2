using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Common;
using UserProfileService.Common.Database;
using UserProfileService.Common.DataBase;
using UserProfileService.Models;

namespace UserProfileService.Features.Settings.UpdateSettings.UpdateUserPreferences;

public class UpdateUserPreferencesHandler : IRequestHandler<UpdateUserPreferencesCommand, Result>
{
    private readonly IGenericRepository<UserPreferences> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserPreferencesHandler(IGenericRepository<UserPreferences> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            int rows = await _repository.GetQueryable()
             .Where(up => up.Id == request.UserId)
             .ExecuteUpdateAsync(setter => setter
                 .SetProperty(up => up.Language, up => request.Language ?? up.Language)
                 .SetProperty(up => up.Theme, up => request.Theme ?? up.Theme)
                 .SetProperty(up => up.WeightUnit, up => request.WeightUnit ?? up.WeightUnit)
                 .SetProperty(up => up.HeightUnit, up => request.HeightUnit ?? up.HeightUnit)
                 .SetProperty(up => up.DistanceUnit, up => request.DistanceUnit ?? up.DistanceUnit),
             cancellationToken);

            if (rows == 0)
                return Result.Failure(Error.NotFound("PreferencesNotFound", "User preferences not found."));

            return Result.Success();
        }, cancellationToken);
    }
}
