namespace ProgressTrackingService.Common;

/// <summary>
///     Maps the "ServiceUrls" section from appsettings.json.
///     Each property holds the base URL for an external service.
/// </summary>
public class ServiceUrls
{
    public const string SectionName = "ServiceUrls";

    public string AuthenticationService { get; set; } = string.Empty;
    public string WorkoutService { get; set; } = string.Empty;
    public string NutritionService { get; set; } = string.Empty;
    public string FitnessCalculationService { get; set; } = string.Empty;
    public string UserProfileService { get; set; } = string.Empty;
}