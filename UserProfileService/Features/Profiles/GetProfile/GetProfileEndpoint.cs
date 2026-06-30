using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;

namespace UserProfileService.Features.Profiles.GetProfile;

public static class GetProfileEndpoint
{
    public static IEndpointRouteBuilder MapGetProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/profiles/me", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProfileQuery(user.GetUserId()));
            return result.ToHttpResult();
        })
        .WithName("GetProfile")
        .WithTags("Profiles")
        .RequireAuthorization();

        return app;
    }
}
