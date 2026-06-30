namespace UserProfileService.Features.Settings.GetSettings;

public class GetSettingsResponse
{
    public UserPreferencesDto UserPreferences { get; set; } = null!;
    public NotificationSettingsDto NotificationSettings { get; set; } = null!;
    public PrivacySettingsDto PrivacySettings { get; set; } = null!;
}

public class UserPreferencesDto
{
    public string Language { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string WeightUnit { get; set; } = string.Empty;
    public string HeightUnit { get; set; } = string.Empty;
    public string DistanceUnit { get; set; } = string.Empty;
}

public class NotificationSettingsDto
{
    public bool WorkoutReminders { get; set; }
    public bool MealReminders { get; set; }
    public bool AchievementAlerts { get; set; }
    public bool WeeklyReports { get; set; }
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
}

public class PrivacySettingsDto
{
    public string ProfileVisibility { get; set; } = string.Empty;
    public bool ShowProgressToFriends { get; set; }
    public bool AllowDataSharing { get; set; }
}
