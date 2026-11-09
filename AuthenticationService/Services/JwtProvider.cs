using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AuthenticationService.Models.Responses;
using AuthenticationService.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Services;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, int ExpiresIn) GenerateToken(UserDto applicationUser)
    {
        var now = DateTime.UtcNow;
        var iatSeconds = EpochTime.GetIntDate(now);

        Claim[] claims =
        {
            new(JwtRegisteredClaimNames.Sub, applicationUser.Id),
            new(JwtRegisteredClaimNames.Email, applicationUser.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, iatSeconds.ToString(), ClaimValueTypes.Integer64)
        };

        var rsaKey = RsaKeyService.GetPrivateKey(_options.PrivateKeyPath, _options.KeyId);
        var signingCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now,
            NotBefore = now.AddSeconds(-5),
            Expires = now.AddMinutes(_options.ExpirationInMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (Token: tokenHandler.WriteToken(token), ExpiresIn: _options.ExpirationInMinutes * 60);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string? ValidateToken(string token)
    {
        var principal = GetValidatedPrincipal(token);
        return principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public ClaimsPrincipal? GetValidatedPrincipal(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var rsaKey = RsaKeyService.GetPublicKey(_options.PublicKeyPath, _options.KeyId);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKey = rsaKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = _options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _options.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string? GetUserIdFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var rsaKey = RsaKeyService.GetPublicKey(_options.PublicKeyPath, _options.KeyId);

        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = rsaKey,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = false
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
