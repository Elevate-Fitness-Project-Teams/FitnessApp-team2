using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string SecretKey { get; set; } = string.Empty;

    [Required] public string Issuer { get; set; } = string.Empty;

    [Required] public string Audience { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Range Is Not Valid")]
    public int ExpirationInMinutes { get; set; }
}