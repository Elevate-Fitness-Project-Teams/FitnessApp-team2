using MediatR;
using UserProfileService.Common;

namespace UserProfileService.Features.Settings.UpdateSettings.UpdateNotificationSettings;

public record UpdateNotificationSettingsCommand(
    int UserId,
    bool? WorkoutReminders,
    bool? MealReminders,
    bool? AchievementAlerts,
    bool? WeeklyReports,
    bool? EmailNotifications,
    bool? PushNotifications
) : IRequest<Result>;
