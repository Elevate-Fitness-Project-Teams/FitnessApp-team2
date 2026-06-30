using MediatR;
using UserProfileService.Common;
using UserProfileService.Features.Profiles.GetProfile;

public record GetProfileQuery(int UserId) : IRequest<Result<GetProfileResponse>>;