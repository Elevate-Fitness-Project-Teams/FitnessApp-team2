using AuthenticationService.Common;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.ValidateToken;

public class ValidateTokenCommandHandler : IRequestHandler<ValidateTokenCommand, Result>
{
    private readonly IJwtProvider _jwtProvider;

    public ValidateTokenCommandHandler(IJwtProvider jwtProvider)
    {
        _jwtProvider = jwtProvider;
    }

    public async Task<Result> Handle(ValidateTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtProvider.ValidateToken(request.Token);

        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure(Error.Failure(AuthErrorCodes.InvalidCredentials, "Invalid or expired token"));
        }

        return Result.Success();
    }
}
