namespace UserProfileService.Models
{
    public class NotificationSettings
    {
        public string Id { get; set; } = string.Empty; // PK and FK -> UserProfile.Id
        public bool WorkoutReminders { get; set; } = true;
        public bool MealReminders { get; set; } = true;
        public bool AchievementAlerts { get; set; } = true;
        public bool WeeklyReports { get; set; } = true;
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;

        // Navigation Property for 1:1 relationship
        public UserProfile UserProfile { get; set; } = null!;
    }
}
