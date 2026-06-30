using System.Security.Claims;

namespace UserProfileService.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? user.FindFirst("sub")?.Value 
            ?? user.FindFirst("UserId")?.Value;

        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User ID claim is missing or invalid in the token.");
    }
}
