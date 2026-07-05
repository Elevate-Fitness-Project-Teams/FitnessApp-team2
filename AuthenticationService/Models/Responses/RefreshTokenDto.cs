namespace AuthenticationService.Models.Responses;

public record RefreshTokenDto(
    int Id,
    string UserId,
    string Token,
    DateTime ExpiresAt,
    DateTime? RevokedAt);