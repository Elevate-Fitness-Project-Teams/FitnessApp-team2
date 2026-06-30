using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.Requests;

public class SendOtpRequest
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
}