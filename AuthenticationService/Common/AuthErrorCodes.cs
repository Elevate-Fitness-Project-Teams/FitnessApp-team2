namespace AuthenticationService.Common;

public static class AuthErrorCodes
{
    public const string DuplicateUserName = "DuplicateUserName";
    public const string DuplicateEmail = "DuplicateEmail";
    public const string AccountLocked = "AUTH_ACCOUNT_LOCKED";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string UserNotFound = "RES_NOT_FOUND";
    public const string EmailNotConfirmed = "AUTH_EMAIL_NOT_CONFIRMED";
}