using MediatR;
using UserProfileService.Common;

namespace UserProfileService.Features.Profiles.UploadProfilePicture;

public record UploadProfilePictureCommand(string UserId, IFormFile ProfilePicture) : IRequest<Result<string>>;
