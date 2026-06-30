using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Common;
using UserProfileService.Common.Database;
using UserProfileService.Models;

namespace UserProfileService.Features.Settings.GetSettings;

public class GetSettingsHandler : IRequestHandler<GetSettingsQuery, Result<GetSettingsResponse>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetSettingsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetSettingsResponse>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _dbContext.UserProfiles
            .Include(up => up.UserPreferences)
            .Include(up => up.NotificationSettings)
            .Include(up => up.PrivacySettings)
            .FirstOrDefaultAsync(up => up.Id == request.UserId, cancellationToken);

        if (userProfile == null)
            return Result<GetSettingsResponse>.Failure(Error.NotFound("ProfileNotFound", "User profile not found."));

        var response = new GetSettingsResponse
        {
            UserPreferences = new UserPreferencesDto
            {
                Language = userProfile.UserPreferences.Language,
                Theme = userProfile.UserPreferences.Theme,
                WeightUnit = userProfile.UserPreferences.WeightUnit,
                HeightUnit = userProfile.UserPreferences.HeightUnit,
                DistanceUnit = userProfile.UserPreferences.DistanceUnit
            },
            NotificationSettings = new NotificationSettingsDto
            {
                WorkoutReminders = userProfile.NotificationSettings.WorkoutReminders,
                MealReminders = userProfile.NotificationSettings.MealReminders,
                AchievementAlerts = userProfile.NotificationSettings.AchievementAlerts,
                WeeklyReports = userProfile.NotificationSettings.WeeklyReports,
                EmailNotifications = userProfile.NotificationSettings.EmailNotifications,
                PushNotifications = userProfile.NotificationSettings.PushNotifications
            },
            PrivacySettings = new PrivacySettingsDto
            {
                ProfileVisibility = userProfile.PrivacySettings.ProfileVisibility,
                ShowProgressToFriends = userProfile.PrivacySettings.ShowProgressToFriends,
                AllowDataSharing = userProfile.PrivacySettings.AllowDataSharing
            }
        };

        return Result<GetSettingsResponse>.Success(response);
    }
}
