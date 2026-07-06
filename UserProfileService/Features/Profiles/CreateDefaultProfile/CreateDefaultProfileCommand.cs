using MediatR;
using UserProfileService.Common;


namespace UserProfileService.Features.Profiles.CreateDefaultProfile;

public record CreateDefaultProfileCommand(string UserId, string FirstName, string LastName, string Email, string PhoneNumber, DateTime RegisteredAt) : IRequest<Result>;
