using Microsoft.EntityFrameworkCore;
using UserProfileService.Models;

namespace UserProfileService.Common.Database;

public static class UserProfileDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        const string testUserId = "11111111-1111-1111-1111-111111111111";

        var existingProfile = await context.UserProfiles.AnyAsync(p => p.Id == testUserId);
        if (!existingProfile)
        {
            var profile = new UserProfile
            {
                Id = testUserId,
                FirstName = "Test",
                LastName = "User",
                Email = "testuser@elevate.com",
                PhoneNumber = "+1234567890",
                IsPremiumCached = false,
                MemberSince = DateTime.UtcNow.AddMonths(-1),
                UserPreferences = new UserPreferences
                {
                    Id = testUserId,
                    Language = "en",
                    Theme = "dark",
                    WeightUnit = "kg",
                    HeightUnit = "cm",
                    DistanceUnit = "km"
                },
                NotificationSettings = new NotificationSettings
                {
                    Id = testUserId,
                    WorkoutReminders = true,
                    MealReminders = true,
                    AchievementAlerts = true,
                    WeeklyReports = true,
                    EmailNotifications = true,
                    PushNotifications = true
                },
                PrivacySettings = new PrivacySettings
                {
                    Id = testUserId,
                    ProfileVisibility = "public",
                    ShowProgressToFriends = true,
                    AllowDataSharing = false
                }
            };

            await context.UserProfiles.AddAsync(profile);
            await context.SaveChangesAsync();
            logger.LogInformation("Successfully seeded default profile and preferences for test user: {UserId}", testUserId);
        }
    }
}
