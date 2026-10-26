using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ProgressTrackingService.Common.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            return null;

        var claimValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue("id")
            ?? principal.FindFirstValue(ClaimTypes.Sid);

        if (string.IsNullOrWhiteSpace(claimValue))
            return null;

        return Guid.TryParse(claimValue, out var guid) ? guid : null;
    }

    public static Guid? GetUserId(this HttpContext? context)
    {
        return context?.User.GetUserId();
    }
}
