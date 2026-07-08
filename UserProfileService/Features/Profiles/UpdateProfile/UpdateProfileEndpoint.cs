using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;
using UserProfileService.Common.Filters;

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
        .AddEndpointFilter<VerifyTokenEndpointFilter>();
        return app;
    }
}

public record UpdateProfileRequest(string FirstName, string LastName, string Email, string PhoneNumber);
