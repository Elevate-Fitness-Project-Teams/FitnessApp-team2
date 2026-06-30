using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;

namespace UserProfileService.Features.Settings.GetSettings;

public static class GetSettingsEndpoint
{
    public static IEndpointRouteBuilder MapGetSettingsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/profiles/me/settings", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSettingsQuery(user.GetUserId()));
            return result.ToHttpResult();
        })
        .WithName("GetSettings")
        .WithTags("Settings")
        .RequireAuthorization();

        return app;
    }
}
