using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProgressTrackingService.Common.Errors;
using ProgressTrackingService.Common.Extensions;

namespace ProgressTrackingService.Common.Security;

public static class UserAccessGuard
{
    public static bool CanAccess(ClaimsPrincipal? caller, Guid targetUserId)
    {
        if (caller == null || !caller.Identity?.IsAuthenticated == true)
            return false;

        if (caller.IsInRole("Admin"))
            return true;

        var callerId = caller.GetUserId();
        return callerId.HasValue && callerId.Value == targetUserId;
    }

    public static Result ValidateAccess(ClaimsPrincipal? caller, Guid targetUserId)
    {
        if (caller == null || caller.Identity?.IsAuthenticated != true)
        {
            return Result.Failure(Error.Unauthorized(ProgressErrors.AuthTokenInvalid, "Authentication required."));
        }

        if (CanAccess(caller, targetUserId))
        {
            return Result.Success();
        }

        return Result.Failure(Error.Forbidden(ProgressErrors.PermForbidden, "Access denied: You can only access your own fitness records."));
    }

    public static IResult? EnsureAccess(this HttpContext context, Guid targetUserId)
    {
        var validation = ValidateAccess(context.User, targetUserId);
        if (validation.IsSuccess)
        {
            return null;
        }

        return validation.ToHttpResult();
    }
}
