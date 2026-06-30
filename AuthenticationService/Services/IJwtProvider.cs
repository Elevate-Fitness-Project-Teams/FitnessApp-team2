using AuthenticationService.Data.Entities;

namespace AuthenticationService.Services;

public interface IJwtProvider
{
    (string Token, int ExpiresIn) GenerateToken(User applicationUser);
    string GenerateRefreshToken();
    string? ValidateToken(string token);
    string? GetUserIdFromExpiredToken(string token);
}