namespace AuthenticationService.Services;

public interface IGrpcIntegrationService
{
    Task<bool> HasCompletedProfileAsync(string userId);
    Task<bool> IsPremiumUserAsync(string userId);
}