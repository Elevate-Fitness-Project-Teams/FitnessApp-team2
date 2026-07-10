using MediatR;
using UserProfileService.Common;
using UserProfileService.Features.Profiles.GetProfile;

public record GetProfileQuery(string UserId) : IRequest<Result<GetProfileResponse>>;