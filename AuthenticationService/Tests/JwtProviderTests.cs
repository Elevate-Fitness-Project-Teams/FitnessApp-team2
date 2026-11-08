using System.IdentityModel.Tokens.Jwt;
using AuthenticationService.Models.Responses;
using AuthenticationService.Options;
using AuthenticationService.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace AuthenticationService.Tests;

public class JwtProviderTests
{
    private readonly JwtProvider _jwtProvider;
    private readonly JwtOptions _options;

    public JwtProviderTests()
    {
        var privateKeyPath = FindKeyFile("auth-service-private.pem");
        var publicKeyPath = FindKeyFile("auth-service-public.pem");

        _options = new JwtOptions
        {
            PrivateKeyPath = privateKeyPath,
            PublicKeyPath = publicKeyPath,
            KeyId = "elevate-fitness-key-1",
            Issuer = "ElevateFitness",
            Audience = "ElevateFitnessUsers",
            ExpirationInMinutes = 15
        };

        _jwtProvider = new JwtProvider(Microsoft.Extensions.Options.Options.Create(_options));
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
    public void GenerateToken_ShouldIncludeExpectedClaims_And15MinuteExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "athlete@elevatefitness.com";
        var userDto = new UserDto
        {
            Id = userId,
            Email = email,
            FirstName = "Athlete",
            LastName = "Fitness",
            EmailConfirmed = true
        };

        // Act
        var result = _jwtProvider.GenerateToken(userDto);

        // Assert
        Assert.NotNull(result.Token);
        Assert.Equal(15 * 60, result.ExpiresIn);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);

        Assert.Equal(userId, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Iat);
        Assert.Equal("ElevateFitness", jwt.Issuer);
        Assert.Contains("ElevateFitnessUsers", jwt.Audiences);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsUserId()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userDto = new UserDto
        {
            Id = userId,
            Email = "athlete@elevatefitness.com",
            FirstName = "Athlete",
            LastName = "Fitness",
            EmailConfirmed = true
        };
        var generated = _jwtProvider.GenerateToken(userDto);

        // Act
        var validatedUserId = _jwtProvider.ValidateToken(generated.Token);

        // Assert
        Assert.Equal(userId, validatedUserId);
    }

    [Fact]
    public void ValidateToken_WithGarbageToken_ReturnsNull()
    {
        // Act
        var validatedUserId = _jwtProvider.ValidateToken("not.a.valid.jwt.token");

        // Assert
        Assert.Null(validatedUserId);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyBase64String()
    {
        // Act
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));
        var bytes = Convert.FromBase64String(refreshToken);
        Assert.Equal(32, bytes.Length);
    }
}
