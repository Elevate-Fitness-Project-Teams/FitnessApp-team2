using MediatR;
using UserProfileService.Common;

namespace UserProfileService.Features.Profiles.UploadProfilePicture;

public record UploadProfilePictureCommand(int UserId, IFormFile ProfilePicture) : IRequest<Result<string>>;
