using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.Requests;

public class LogoutRequest
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}