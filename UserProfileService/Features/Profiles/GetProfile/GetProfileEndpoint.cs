using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;
using UserProfileService.Common.Filters;

namespace UserProfileService.Features.Profiles.GetProfile;

public static class GetProfileEndpoint
{
    public static IEndpointRouteBuilder MapGetProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/profiles", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = user.GetUserId();
            var query = new GetProfileQuery(userId);
            var result = await mediator.Send(query);
            return result.ToHttpResult();
        })
        .WithName("GetProfile")
        .WithTags("Profiles")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AddEndpointFilter<VerifyTokenEndpointFilter>();
        return app;
    }
}
