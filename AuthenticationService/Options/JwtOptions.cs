using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string PrivateKeyPath { get; set; } = string.Empty;

    [Required] public string PublicKeyPath { get; set; } = string.Empty;

    [Required] public string KeyId { get; set; } = string.Empty;

    [Required] public string Issuer { get; set; } = string.Empty;

    [Required] public string Audience { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Range Is Not Valid")]
    public int ExpirationInMinutes { get; set; }
}