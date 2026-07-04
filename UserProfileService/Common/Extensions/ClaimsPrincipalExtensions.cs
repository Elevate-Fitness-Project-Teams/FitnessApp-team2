using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UserProfileService.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
    user.FindFirstValue(JwtRegisteredClaimNames.Sub)
    ?? throw new UnauthorizedAccessException("Missing sub claim.");
}
