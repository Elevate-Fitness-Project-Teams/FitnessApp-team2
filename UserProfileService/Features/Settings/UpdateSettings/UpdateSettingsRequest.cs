namespace UserProfileService.Features.Settings.UpdateSettings;

public record UpdateSettingsRequest(
    UpdateUserPreferencesDto? UserPreferences,
    UpdateNotificationSettingsDto? NotificationSettings,
    UpdatePrivacySettingsDto? PrivacySettings
);
