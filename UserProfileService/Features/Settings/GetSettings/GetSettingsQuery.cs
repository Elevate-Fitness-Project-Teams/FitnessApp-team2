using MediatR;
using UserProfileService.Common;
using UserProfileService.Features.Settings.GetSettings;

public record GetSettingsQuery(int UserId) : IRequest<Result<GetSettingsResponse>>;
