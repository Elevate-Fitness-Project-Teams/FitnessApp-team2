using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Common;
using UserProfileService.Common.Database;

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
        var response = await _dbContext.UserProfiles
            .Where(up => up.Id == request.UserId)
            .Select(up => new GetSettingsResponse
            {
                // EF Core will automatically JOIN UserPreferences and select these specific columns
                UserPreferences = new UserPreferencesDto
                {
                    Language = up.UserPreferences.Language,
                    Theme = up.UserPreferences.Theme,
                    WeightUnit = up.UserPreferences.WeightUnit,
                    HeightUnit = up.UserPreferences.HeightUnit,
                    DistanceUnit = up.UserPreferences.DistanceUnit
                },

                // EF Core will automatically JOIN NotificationSettings and select these specific columns
                NotificationSettings = new NotificationSettingsDto
                {
                    WorkoutReminders = up.NotificationSettings.WorkoutReminders,
                    MealReminders = up.NotificationSettings.MealReminders,
                    AchievementAlerts = up.NotificationSettings.AchievementAlerts,
                    WeeklyReports = up.NotificationSettings.WeeklyReports,
                    EmailNotifications = up.NotificationSettings.EmailNotifications,
                    PushNotifications = up.NotificationSettings.PushNotifications
                },

                // EF Core will automatically JOIN PrivacySettings and select these specific columns
                PrivacySettings = new PrivacySettingsDto
                {
                    ProfileVisibility = up.PrivacySettings.ProfileVisibility,
                    ShowProgressToFriends = up.PrivacySettings.ShowProgressToFriends,
                    AllowDataSharing = up.PrivacySettings.AllowDataSharing
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
        {
            return Result<GetSettingsResponse>.Failure(Error.NotFound("ProfileNotFound", "User profile not found."));
        }

        return Result<GetSettingsResponse>.Success(response);
    }
}
