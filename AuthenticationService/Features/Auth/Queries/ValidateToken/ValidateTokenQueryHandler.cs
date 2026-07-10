using AuthenticationService.Common;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.ValidateToken;

public class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, Result>
{
    private readonly IJwtProvider _jwtProvider;

    public ValidateTokenQueryHandler(IJwtProvider jwtProvider)
    {
        _jwtProvider = jwtProvider;
    }

    public async Task<Result> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        var userId = _jwtProvider.ValidateToken(request.Token);

        if (string.IsNullOrEmpty(userId))
            return Result.Failure(Error.Failure(AuthErrorCodes.InvalidCredentials, "Invalid or expired token"));

        return Result.Success();
    }
}