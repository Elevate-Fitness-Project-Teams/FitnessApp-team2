using MediatR;
using UserProfileService.Common;
namespace UserProfileService.Features.Profiles.UpdateProfile;

public record UpdateProfileCommand(int UserId, string FirstName, string LastName, string Email, string PhoneNumber) : IRequest<Result>;