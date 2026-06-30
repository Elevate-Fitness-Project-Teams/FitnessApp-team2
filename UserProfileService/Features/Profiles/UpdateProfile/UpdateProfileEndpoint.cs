using MediatR;
using System.Security.Claims;
using UserProfileService.Common.Extensions;

namespace UserProfileService.Features.Profiles.UpdateProfile;

public static class UpdateProfileEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/profiles/me", async (UpdateProfileRequest request, ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateProfileCommand(
                user.GetUserId(),
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber));
            return result.ToHttpResult();
        })
        .WithName("UpdateProfile")
        .WithTags("Profiles")
        .RequireAuthorization();

        return app;
    }
}

public record UpdateProfileRequest(string FirstName, string LastName, string Email, string PhoneNumber);
