namespace UserProfileService.Models
{
    public class PrivacySettings
    {
        public int Id { get; set; } // PK and FK -> UserProfile.Id
        public string ProfileVisibility { get; set; } = "private";
        public bool ShowProgressToFriends { get; set; } = false;
        public bool AllowDataSharing { get; set; } = false;

        // Navigation Property for 1:1 relationship
        public UserProfile UserProfile { get; set; } = null!;
    }
}
