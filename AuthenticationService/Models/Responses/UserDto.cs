namespace AuthenticationService.Models.Responses;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsLockedOut { get; set; }
    public DateTime? LockedUntil { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}
