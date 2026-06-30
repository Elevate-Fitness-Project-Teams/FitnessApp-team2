using System;

namespace UserProfileService.Models
{
    public class UserProfile
    {
        public int Id { get; set; } // PK, matches Users.Id from Auth Service
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public bool IsPremiumCached { get; set; }
        public DateTime MemberSince { get; set; }

        // Navigation Properties for 1:1 relationships
        public UserPreferences UserPreferences { get; set; } = null!;
        public NotificationSettings NotificationSettings { get; set; } = null!;
        public PrivacySettings PrivacySettings { get; set; } = null!;
    }
}
