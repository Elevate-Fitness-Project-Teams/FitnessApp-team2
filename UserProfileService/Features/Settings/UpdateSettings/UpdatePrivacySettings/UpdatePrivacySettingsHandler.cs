
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Common;
using UserProfileService.Common.Database;
using UserProfileService.Common.DataBase;
using UserProfileService.Models;

namespace UserProfileService.Features.Settings.UpdateSettings.UpdatePrivacySettings;

public class UpdatePrivacySettingsHandler : IRequestHandler<UpdatePrivacySettingsCommand, Result>
{
    private readonly IGenericRepository<PrivacySettings> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePrivacySettingsHandler(IGenericRepository<PrivacySettings> repository, IUnitOfWork unitOfWork = null)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdatePrivacySettingsCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
        {
            var rows = await _repository.GetQueryable()
             .Where(ps => ps.Id == request.UserId)
             .ExecuteUpdateAsync(setter => setter
                 .SetProperty(ps => ps.ProfileVisibility, ps => request.ProfileVisibility ?? ps.ProfileVisibility)
                 .SetProperty(ps => ps.ShowProgressToFriends, ps => request.ShowProgressToFriends ?? ps.ShowProgressToFriends)
                 .SetProperty(ps => ps.AllowDataSharing, ps => request.AllowDataSharing ?? ps.AllowDataSharing),
             cancellationToken);

            if (rows == 0)
                return Result.Failure(Error.NotFound("PrivacySettingsNotFound", "Privacy settings not found."));

            return Result.Success();
        }, cancellationToken);
    }
}
