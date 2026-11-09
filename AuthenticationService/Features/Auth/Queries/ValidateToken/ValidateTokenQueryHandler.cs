using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthenticationService.Common;
using AuthenticationService.Services;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.ValidateToken;

public class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, Result>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly ITokenRevocationStore _tokenRevocationStore;

    public ValidateTokenQueryHandler(IJwtProvider jwtProvider, ITokenRevocationStore tokenRevocationStore)
    {
        _jwtProvider = jwtProvider;
        _tokenRevocationStore = tokenRevocationStore;
    }

    public async Task<Result> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
    {
        var principal = _jwtProvider.GetValidatedPrincipal(request.Token);

        if (principal == null)
            return Result.Failure(Error.Failure(AuthErrorCodes.InvalidCredentials, "Invalid or expired token"));

        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        Guid? userId = Guid.TryParse(sub, out var parsedGuid) ? parsedGuid : null;

        var iatClaim = principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
        long? issuedAtUnix = long.TryParse(iatClaim, out var parsedIat) ? parsedIat : null;

        var isRevoked = await _tokenRevocationStore.IsTokenRevokedAsync(jti, userId, issuedAtUnix, cancellationToken);
        if (isRevoked)
        {
            return Result.Failure(Error.Unauthorized(AuthErrorCodes.TokenRevoked, "Token has been revoked"));
        }

        return Result.Success();
    }
}
