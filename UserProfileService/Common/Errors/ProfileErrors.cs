namespace UserProfileService.Common.Errors;

public static class ProfileErrors
{
    public const string ProfileNotFound = "User profile was not found.";
    public const string SettingsNotFound = "User settings were not found.";
    public const string InvalidFileType = "Only JPG and PNG files are allowed.";
    public const string FileTooLarge = "File size must not exceed 5MB.";
    public const string PasswordChangeFailed = "Password change request failed.";
}
