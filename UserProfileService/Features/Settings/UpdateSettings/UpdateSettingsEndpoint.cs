using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;

namespace UserProfileService.Features.Settings.UpdateSettings;

public static class UpdateSettingsEndpoint
{
    public static IEndpointRouteBuilder MapUpdateSettingsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/profiles/me/settings", async (UpdateSettingsRequest request, ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateSettingsOrchestrator(
                user.GetUserId(),
                request.UserPreferences,
                request.NotificationSettings,
                request.PrivacySettings));
            return result.ToHttpResult();
        })
        .WithName("UpdateSettings")
        .WithTags("Settings")
        .RequireAuthorization();

        return app;
    }
}

