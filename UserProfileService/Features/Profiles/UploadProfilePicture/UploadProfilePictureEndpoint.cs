using MediatR;
using UserProfileService.Common.Extensions;
using UserProfileService.Common.Filters;

namespace UserProfileService.Features.Profiles.UploadProfilePicture;

public static class UploadProfilePictureEndpoint
{
    public static IEndpointRouteBuilder MapUploadProfilePictureEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/profiles/picture", async (IFormFile profilePicture, HttpContext httpContext, IMediator mediator) =>
        {
            var result = await mediator.Send(new UploadProfilePictureCommand(httpContext.User.GetUserId(), profilePicture));
            return result.ToHttpResult();
        })
        .WithName("UploadProfilePicture")
        .WithTags("Profiles")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AddEndpointFilter<VerifyTokenEndpointFilter>();
        return app;
    }
}
