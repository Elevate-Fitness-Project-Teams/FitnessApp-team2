using MediatR;
using UserProfileService.Common;

namespace UserProfileService.Features.Settings.UpdateSettings;

public record UpdateSettingsOrchestrator(
    int UserId,
    UpdateUserPreferencesDto? UserPreferences,
    UpdateNotificationSettingsDto? NotificationSettings,
    UpdatePrivacySettingsDto? PrivacySettings
) : IRequest<Result>;

public record UpdateUserPreferencesDto(
    string? Language,
    string? Theme,
    string? WeightUnit,
    string? HeightUnit,
    string? DistanceUnit
);

public record UpdateNotificationSettingsDto(
    bool? WorkoutReminders,
    bool? MealReminders,
    bool? AchievementAlerts,
    bool? WeeklyReports,
    bool? EmailNotifications,
    bool? PushNotifications
);

public record UpdatePrivacySettingsDto(
    string? ProfileVisibility,
    bool? ShowProgressToFriends,
    bool? AllowDataSharing
);
