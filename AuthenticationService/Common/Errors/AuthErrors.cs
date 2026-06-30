namespace AuthenticationService.Common.Errors;

public static class AuthErrors
{
    public const string EmailAlreadyInUse = "Email is already in use.";
    public const string UserAlreadyExists = "User already exists";
    public const string RegistrationFailed = "Registration failed";
    public const string InvalidCredentials = "Invalid email or password.";
    public const string AccountLockedOut = "Account is locked out. Please try again later.";
    public const string EmailNotConfirmed = "Email has not been confirmed.";
    public const string UserNotFound = "User not found.";
    public const string InvalidToken = "The provided token is invalid or expired.";
    public const string PasswordResetFailed = "Password reset failed.";
}