using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;


namespace UserProfileService.Features.Profiles.GetProfile;

public static class GetProfileEndpoint
{
    public static IEndpointRouteBuilder MapGetProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/profiles", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.GetUserId();
            var query = new GetProfileQuery(userId);
            var result = await mediator.Send(query);
            return result.ToHttpResult();
        })
        .WithName("GetProfile")
        .WithTags("Profiles")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
        return app;
    }
}
