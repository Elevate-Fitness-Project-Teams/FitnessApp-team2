using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;

namespace UserProfileService.Features.Profiles.UploadProfilePicture;

public static class UploadProfilePictureEndpoint
{
    public static IEndpointRouteBuilder MapUploadProfilePictureEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/profiles/me/picture", async (IFormFile profilePicture, ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new UploadProfilePictureCommand(user.GetUserId(), profilePicture));
            return result.ToHttpResult();
        })
        .WithName("UploadProfilePicture")
        .WithTags("Profiles")
        .DisableAntiforgery()
        .RequireAuthorization();

        return app;
    }
}
