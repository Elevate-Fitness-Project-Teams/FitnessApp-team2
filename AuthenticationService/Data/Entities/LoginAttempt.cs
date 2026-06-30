namespace AuthenticationService.Data.Entities;

public class LoginAttempt
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; }
    public bool IsSuccess { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}