namespace AuthenticationService.Models.Requests;

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}