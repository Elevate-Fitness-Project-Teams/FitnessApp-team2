using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Common;
using UserProfileService.Common.Database;
using UserProfileService.Models;

namespace UserProfileService.Features.Profiles.GetProfile;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, Result<GetProfileResponse>>
{
    private readonly IGenericRepository<UserProfile> _userProfileRepository;

    public GetProfileHandler(IGenericRepository<UserProfile> userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public async Task<Result<GetProfileResponse>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _userProfileRepository.GetQueryable()
            .FirstOrDefaultAsync(up => up.Id == request.UserId, cancellationToken);

        if (userProfile == null)
        {
            var error = Error.Failure("UserProfileNotFound", $"User profile with ID {request.UserId} not found.");
            return Result<GetProfileResponse>.Failure(error);
        }

        var response = new GetProfileResponse
        {
            Id = userProfile.Id,
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            Email = userProfile.Email,
            PhoneNumber = userProfile.PhoneNumber,
            ProfilePictureUrl = userProfile.ProfilePictureUrl,
            IsPremiumCached = userProfile.IsPremiumCached,
            MemberSince = userProfile.MemberSince
        };

        return Result<GetProfileResponse>.Success(response);
    }
}
