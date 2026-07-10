using MediatR;
using UserProfileService.Common;

namespace UserProfileService.Features.Settings.UpdateSettings.UpdatePrivacySettings;

public record UpdatePrivacySettingsCommand(
    string UserId,
    string? ProfileVisibility,
    bool? ShowProgressToFriends,
    bool? AllowDataSharing
) : IRequest<Result>;
