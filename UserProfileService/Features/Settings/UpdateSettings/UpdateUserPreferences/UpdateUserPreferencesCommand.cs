using MediatR;
using UserProfileService.Common;

namespace UserProfileService.Features.Settings.UpdateSettings.UpdateUserPreferences;

public record UpdateUserPreferencesCommand(
    int UserId,
    string? Language,
    string? Theme,
    string? WeightUnit,
    string? HeightUnit,
    string? DistanceUnit
) : IRequest<Result>;
