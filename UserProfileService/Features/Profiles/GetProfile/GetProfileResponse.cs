namespace UserProfileService.Features.Profiles.GetProfile;

public class GetProfileResponse
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool IsPremiumCached { get; set; }
    public DateTime MemberSince { get; set; }
}
