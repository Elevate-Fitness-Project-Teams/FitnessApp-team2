namespace AuthenticationService.Services;

public interface ITokenRevocationStore
{
    /// <summary>
    /// Revokes an individual access token by its unique identifier (jti) for the remaining token lifetime.
    /// </summary>
    Task RevokeTokenAsync(string jti, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>
    /// Revokes all active access tokens for a user issued at or before the specified cutoff timestamp.
    /// Used during password change, password reset, account lockout, or "Logout All Devices".
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, DateTimeOffset cutoffUtc, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a token has been revoked, either individually via jti or collectively via user cutoff.
    /// </summary>
    Task<bool> IsTokenRevokedAsync(string? jti, Guid? userId = null, long? issuedAtUnix = null, CancellationToken ct = default);
}
