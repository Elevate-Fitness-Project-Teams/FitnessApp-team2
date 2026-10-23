namespace ProgressTrackingService.Common.Errors;

public static class ProgressErrors
{
    // Resource errors
    public const string ResSessionNotFound = "RES_SESSION_NOT_FOUND";
    public const string ResUserNotFound = "RES_USER_NOT_FOUND";
    public const string ResAchievementNotFound = "RES_ACHIEVEMENT_NOT_FOUND";

    // Validation errors
    public const string ValRequiredField = "VAL_REQUIRED_FIELD";
    public const string ValInvalidWeight = "VAL_INVALID_WEIGHT";
    public const string ValInvalidDate = "VAL_INVALID_DATE";
    public const string ValInvalidDifficulty = "VAL_INVALID_DIFFICULTY";
    public const string ValInvalidRating = "VAL_INVALID_RATING";
    public const string ValInvalidPeriod = "VAL_INVALID_PERIOD";

    // Auth & Permission errors
    public const string AuthTokenInvalid = "AUTH_TOKEN_INVALID";
    public const string PermForbidden = "PERM_FORBIDDEN";

    // Server errors
    public const string SrvServiceUnavailable = "SRV_SERVICE_UNAVAILABLE";
    public const string SrvDatabaseError = "SRV_DATABASE_ERROR";
    public const string SrvUnexpectedError = "SRV_UNEXPECTED_ERROR";
}
