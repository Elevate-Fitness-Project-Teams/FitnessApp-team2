
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Common;
using UserProfileService.Common.Database;
using UserProfileService.Common.DataBase;
using UserProfileService.Models;

namespace UserProfileService.Features.Profiles.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result>
{

    private readonly IGenericRepository<UserProfile> _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateProfileHandler(IGenericRepository<UserProfile> userProfileRepository, IUnitOfWork unitOfWork)
    {
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteAsync(async () =>
           {
               int affectedRows = await _userProfileRepository.GetQueryable()
               .Where(p => p.Id == request.UserId)
               .ExecuteUpdateAsync(setter => setter
                   .SetProperty(p => p.FirstName, request.FirstName)
                   .SetProperty(p => p.LastName, request.LastName)
                   .SetProperty(p => p.Email, request.Email)
                   .SetProperty(p => p.PhoneNumber, request.PhoneNumber), cancellationToken);
               if (affectedRows == 0)
                   return Result.Failure(Error.NotFound("ProfileNotFound", "User profile not found."));
               return Result.Success();
           }, cancellationToken);
    }
}
