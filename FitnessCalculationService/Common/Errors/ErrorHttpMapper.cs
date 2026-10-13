using Microsoft.AspNetCore.Http;

namespace FitnessCalculationService.Common.Errors;

public static class ErrorHttpMapper
{
    public static int ToStatusCode(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return StatusCodes.Status400BadRequest;

        return errorCode switch
        {
            // 404 Not Found
            ErrorCodes.FceStatsNotFound => StatusCodes.Status404NotFound,
            ErrorCodes.FceNoMatchingPlan => StatusCodes.Status404NotFound,
            ErrorCodes.ResPlanNotFound => StatusCodes.Status404NotFound,

            // 401 Unauthorized
            ErrorCodes.AuthTokenInvalid => StatusCodes.Status401Unauthorized,
            ErrorCodes.AuthTokenExpired => StatusCodes.Status401Unauthorized,

            // 403 Forbidden
            ErrorCodes.PermAccessDenied => StatusCodes.Status403Forbidden,

            // 500 Internal Server Error
            ErrorCodes.SrvDatabaseError => StatusCodes.Status500InternalServerError,
            ErrorCodes.SrvUnexpectedError => StatusCodes.Status500InternalServerError,

            // Prefix fallbacks
            _ when errorCode.StartsWith("VAL_") => StatusCodes.Status400BadRequest,
            _ when errorCode.StartsWith("RES_") => StatusCodes.Status404NotFound,
            _ when errorCode.StartsWith("AUTH_") => StatusCodes.Status401Unauthorized,
            _ when errorCode.StartsWith("PERM_") => StatusCodes.Status403Forbidden,
            _ when errorCode.StartsWith("SRV_") => StatusCodes.Status500InternalServerError,

            // Default
            _ => StatusCodes.Status400BadRequest
        };
    }
}
