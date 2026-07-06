using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;
using UserProfileService.Common.Filters;

namespace UserProfileService.Features.Settings.GetSettings;

public static class GetSettingsEndpoint
{
    public static IEndpointRouteBuilder MapGetSettingsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/profiles/settings", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSettingsQuery(user.GetUserId()));
            return result.ToHttpResult();
        })
        .WithName("GetSettings")
        .WithTags("Settings")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AddEndpointFilter<VerifyTokenEndpointFilter>();
        return app;
    }
}
