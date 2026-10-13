namespace FitnessCalculationService.Common.Errors;

public static class ErrorCodes
{
    // Validation
    public const string ValRequiredField = "VAL_REQUIRED_FIELD";
    public const string ValInvalidAge = "VAL_INVALID_AGE";
    public const string ValInvalidWeight = "VAL_INVALID_WEIGHT";
    public const string ValInvalidHeight = "VAL_INVALID_HEIGHT";
    public const string ValInvalidGender = "VAL_INVALID_GENDER";
    public const string ValInvalidGoal = "VAL_INVALID_GOAL";
    public const string ValInvalidActivity = "VAL_INVALID_ACTIVITY";
    public const string ValInvalidDate = "VAL_INVALID_DATE";

    // Domain / FCE
    public const string FceStatsNotFound = "FCE_STATS_NOT_FOUND";
    public const string FceInvalidCalculation = "FCE_INVALID_CALCULATION";
    public const string FceMetricsNotCalculated = "FCE_METRICS_NOT_CALCULATED";
    public const string FceNoMatchingPlan = "FCE_NO_MATCHING_PLAN";
    public const string ResPlanNotFound = "RES_PLAN_NOT_FOUND";

    // Auth & Permissions
    public const string AuthTokenInvalid = "AUTH_TOKEN_INVALID";
    public const string AuthTokenExpired = "AUTH_TOKEN_EXPIRED";
    public const string PermAccessDenied = "PERM_ACCESS_DENIED";

    // Server
    public const string SrvDatabaseError = "SRV_DATABASE_ERROR";
    public const string SrvUnexpectedError = "SRV_UNEXPECTED_ERROR";
}
