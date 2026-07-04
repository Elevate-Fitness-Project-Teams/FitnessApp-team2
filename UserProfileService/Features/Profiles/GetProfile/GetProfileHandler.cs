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
        var userProfile = await _userProfileRepository.GetQueryable().Where(up => up.Id == request.UserId).
            Select(up => new GetProfileResponse
            {
                Id = up.Id,
                FirstName = up.FirstName,
                LastName = up.LastName,
                Email = up.Email,
                PhoneNumber = up.PhoneNumber,
                ProfilePictureUrl = up.ProfilePictureUrl,
                IsPremiumCached = up.IsPremiumCached,
                MemberSince = up.MemberSince
            }).FirstOrDefaultAsync(cancellationToken);)

        if (userProfile == null)
        {
            var error = Error.Failure("UserProfileNotFound", $"User profile with ID {request.UserId} not found.");
            return Result<GetProfileResponse>.Failure(error);
        }
        return Result<GetProfileResponse>.Success(userProfile);
    }
