using FluentValidation;

namespace UserProfileService.Features.Settings.UpdateSettings;

public class UpdateSettingsValidator : AbstractValidator<UpdateSettingsOrchestrator>
{
    private static readonly string[] ValidLanguages = ["en", "ar", "fr", "es", "de"];
    private static readonly string[] ValidThemes = ["light", "dark"];
    private static readonly string[] ValidWeightUnits = ["kg", "lb"];
    private static readonly string[] ValidHeightUnits = ["cm", "ft"];
    private static readonly string[] ValidDistanceUnits = ["km", "mi"];
    private static readonly string[] ValidVisibilities = ["public", "friends", "private"];

    public UpdateSettingsValidator()
    {
        When(x => x.UserPreferences is not null, () =>
        {
            RuleFor(x => x.UserPreferences!.Language)
                .Must(v => v == null || ValidLanguages.Contains(v))
                .WithMessage($"Language must be one of: {string.Join(", ", ValidLanguages)}.");

            RuleFor(x => x.UserPreferences!.Theme)
                .Must(v => v == null || ValidThemes.Contains(v))
                .WithMessage($"Theme must be one of: {string.Join(", ", ValidThemes)}.");

            RuleFor(x => x.UserPreferences!.WeightUnit)
                .Must(v => v == null || ValidWeightUnits.Contains(v))
                .WithMessage($"Weight unit must be one of: {string.Join(", ", ValidWeightUnits)}.");

            RuleFor(x => x.UserPreferences!.HeightUnit)
                .Must(v => v == null || ValidHeightUnits.Contains(v))
                .WithMessage($"Height unit must be one of: {string.Join(", ", ValidHeightUnits)}.");

            RuleFor(x => x.UserPreferences!.DistanceUnit)
                .Must(v => v == null || ValidDistanceUnits.Contains(v))
                .WithMessage($"Distance unit must be one of: {string.Join(", ", ValidDistanceUnits)}.");
        });

        When(x => x.PrivacySettings is not null, () =>
        {
            RuleFor(x => x.PrivacySettings!.ProfileVisibility)
                .Must(v => v == null || ValidVisibilities.Contains(v))
                .WithMessage($"Profile visibility must be one of: {string.Join(", ", ValidVisibilities)}.");
        });
    }
}
