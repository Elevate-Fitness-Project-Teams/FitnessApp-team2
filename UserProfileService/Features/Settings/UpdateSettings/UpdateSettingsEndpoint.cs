using MediatR;
using UserProfileService.Common.Extensions;
using UserProfileService.Common.Filters;

namespace UserProfileService.Features.Settings.UpdateSettings;

public static class UpdateSettingsEndpoint
{
    public static IEndpointRouteBuilder MapUpdateSettingsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/profiles/settings", async (UpdateSettingsRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateSettingsOrchestrator(
                httpContext.User.GetUserId(),
                request.UserPreferences,
                request.NotificationSettings,
                request.PrivacySettings));
            return result.ToHttpResult();
        })
        .WithName("UpdateSettings")
        .WithTags("Settings")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AddEndpointFilter<VerifyTokenEndpointFilter>();
        return app;
    }
}

