using MediatR;
using UserProfileService.Common;

namespace UserProfileService.Features.Profiles.ChangePassword;

public record ChangePasswordCommand(int UserId, string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest<Result>;
