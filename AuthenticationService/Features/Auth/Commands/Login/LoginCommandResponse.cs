namespace AuthenticationService.Features.Auth.Commands.Login;

public record LoginCommandResponse(
    string Token,
    string RefreshToken,
    bool ProfileCompleted,
    bool IsPremium
);