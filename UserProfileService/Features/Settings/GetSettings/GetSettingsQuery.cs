using MediatR;
using UserProfileService.Common;
using UserProfileService.Features.Settings.GetSettings;

public record GetSettingsQuery(string UserId) : IRequest<Result<GetSettingsResponse>>;
