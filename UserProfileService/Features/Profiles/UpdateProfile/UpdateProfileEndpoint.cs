using MediatR;
using UserProfileService.Common.Extensions;


namespace UserProfileService.Features.Profiles.UpdateProfile;

public static class UpdateProfileEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/profiles", async (UpdateProfileRequest request, HttpContext httpContext, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateProfileCommand(
                httpContext.User.GetUserId(),
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber));
            return result.ToHttpResult();
        })
        .WithName("UpdateProfile")
        .WithTags("Profiles")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
        return app;
    }
}

public record UpdateProfileRequest(string FirstName, string LastName, string Email, string PhoneNumber);
