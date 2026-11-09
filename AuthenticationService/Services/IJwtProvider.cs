using System.Security.Claims;
using AuthenticationService.Models.Responses;

namespace AuthenticationService.Services;

public interface IJwtProvider
{
    (string Token, int ExpiresIn) GenerateToken(UserDto applicationUser);
    string GenerateRefreshToken();
    string? ValidateToken(string token);
    ClaimsPrincipal? GetValidatedPrincipal(string token);
    string? GetUserIdFromExpiredToken(string token);
}
