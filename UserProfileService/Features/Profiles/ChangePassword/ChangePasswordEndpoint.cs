using MediatR;
using UserProfileService.Common.Extensions;

namespace UserProfileService.Features.Profiles.ChangePassword;

public static class ChangePasswordEndpoint
{
    public static IEndpointRouteBuilder MapChangePasswordEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/profiles/me/change-password", async (ChangePasswordRequest request, System.Security.Claims.ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new ChangePasswordCommand(
                user.GetUserId(),
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmPassword));
            return result.ToHttpResult();
        })
        .WithName("ChangePassword")
        .WithTags("Profiles")
        .RequireAuthorization();

        return app;
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
