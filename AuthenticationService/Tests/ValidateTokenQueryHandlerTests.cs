using System.IdentityModel.Tokens.Jwt;
using AuthenticationService.Common;
using AuthenticationService.Features.Auth.Queries.ValidateToken;
using AuthenticationService.Models.Responses;
using AuthenticationService.Options;
using AuthenticationService.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace AuthenticationService.Tests;

public class ValidateTokenQueryHandlerTests
{
    private readonly JwtProvider _jwtProvider;
    private readonly FakeTokenRevocationStore _tokenRevocationStore;
    private readonly ValidateTokenQueryHandler _handler;

    public ValidateTokenQueryHandlerTests()
    {
        var privateKeyPath = FindKeyFile("auth-service-private.pem");
        var publicKeyPath = FindKeyFile("auth-service-public.pem");

        var options = new JwtOptions
        {
            PrivateKeyPath = privateKeyPath,
            PublicKeyPath = publicKeyPath,
            KeyId = "elevate-fitness-key-1",
            Issuer = "ElevateFitness",
            Audience = "ElevateFitnessUsers",
            ExpirationInMinutes = 15
        };

        _jwtProvider = new JwtProvider(Microsoft.Extensions.Options.Options.Create(options));
        _tokenRevocationStore = new FakeTokenRevocationStore();
        _handler = new ValidateTokenQueryHandler(_jwtProvider, _tokenRevocationStore);
    }

    private static string FindKeyFile(string filename)
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "keys", filename),
            Path.Combine(Directory.GetCurrentDirectory(), "../Keys", filename),
            Path.Combine(AppContext.BaseDirectory, "../../../keys", filename),
            Path.Combine(AppContext.BaseDirectory, "../../../../Keys", filename),
            Path.Combine(@"D:\Elevate Fitness\Keys", filename)
        };

        return candidates.FirstOrDefault(File.Exists)
               ?? throw new FileNotFoundException($"Cannot locate key file: {filename}");
    }

    [Fact]
    public async Task Handle_WhenTokenIsValidAndNotRevoked_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userDto = new UserDto
        {
            Id = userId,
            Email = "athlete@elevate.com",
            FirstName = "Athlete",
            LastName = "Fitness",
            EmailConfirmed = true
        };
        var (token, _) = _jwtProvider.GenerateToken(userDto);

        // Act
        var result = await _handler.Handle(new ValidateTokenQuery(token), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenTokenIsMalformed_ReturnsFailure()
    {
        // Act
        var result = await _handler.Handle(new ValidateTokenQuery("invalid-token-string"), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrorCodes.InvalidCredentials, result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenTokenJtiIsRevoked_ReturnsTokenRevokedError()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userDto = new UserDto
        {
            Id = userId,
            Email = "athlete@elevate.com",
            FirstName = "Athlete",
            LastName = "Fitness",
            EmailConfirmed = true
        };
        var (token, _) = _jwtProvider.GenerateToken(userDto);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var jti = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        // Revoke the token by jti
        await _tokenRevocationStore.RevokeTokenAsync(jti, TimeSpan.FromMinutes(15));

        // Act
        var result = await _handler.Handle(new ValidateTokenQuery(token), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrorCodes.TokenRevoked, result.Error.Code);
        Assert.Equal("Token has been revoked", result.Error.Description);
    }

    [Fact]
    public async Task Handle_WhenUserCutoffIsAfterTokenIssuedAt_ReturnsTokenRevokedError()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var userDto = new UserDto
        {
            Id = userGuid.ToString(),
            Email = "athlete@elevate.com",
            FirstName = "Athlete",
            LastName = "Fitness",
            EmailConfirmed = true
        };
        var (token, _) = _jwtProvider.GenerateToken(userDto);

        // User changes password or logs out all devices in the future (or immediately after)
        var cutoff = DateTimeOffset.UtcNow.AddSeconds(1);
        await _tokenRevocationStore.RevokeAllUserTokensAsync(userGuid, cutoff, TimeSpan.FromMinutes(15));

        // Act
        var result = await _handler.Handle(new ValidateTokenQuery(token), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrorCodes.TokenRevoked, result.Error.Code);
    }
}

public class FakeTokenRevocationStore : ITokenRevocationStore
{
    private readonly HashSet<string> _revokedJtis = new();
    private readonly Dictionary<Guid, long> _userCutoffs = new();

    public Task RevokeTokenAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        _revokedJtis.Add(jti);
        return Task.CompletedTask;
    }

    public Task RevokeAllUserTokensAsync(Guid userId, DateTimeOffset cutoffUtc, TimeSpan ttl, CancellationToken ct = default)
    {
        _userCutoffs[userId] = cutoffUtc.ToUnixTimeSeconds();
        return Task.CompletedTask;
    }

    public Task<bool> IsTokenRevokedAsync(string? jti, Guid? userId = null, long? issuedAtUnix = null, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(jti) && _revokedJtis.Contains(jti))
            return Task.FromResult(true);

        if (userId.HasValue && _userCutoffs.TryGetValue(userId.Value, out var cutoff))
        {
            if (!issuedAtUnix.HasValue || issuedAtUnix.Value <= cutoff)
                return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
