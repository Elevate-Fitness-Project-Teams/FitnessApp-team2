namespace UserProfileService.Models
{
    public class UserPreferences
    {
        public string Id { get; set; } = string.Empty; // PK and FK -> UserProfile.Id
        public string Language { get; set; } = "en";
        public string Theme { get; set; } = "light";
        public string WeightUnit { get; set; } = "kg";
        public string HeightUnit { get; set; } = "cm";
        public string DistanceUnit { get; set; } = "km";

        // Navigation Property for 1:1 relationship
        public UserProfile UserProfile { get; set; } = null!;
    }
}
