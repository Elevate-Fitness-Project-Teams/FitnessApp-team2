using FitnessCalculationService.Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessCalculationService.Common.Security;

public class OwnershipFilter : IEndpointFilter
{
    private readonly string _routeParamName;

    public OwnershipFilter(string routeParamName = "userId")
    {
        _routeParamName = routeParamName;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

        if (!currentUser.IsAuthenticated)
        {
            var unauthorized = ApiResponse<object?>.Failure(
                new[] { ErrorCodes.AuthTokenInvalid },
                "Authentication required.",
                StatusCodes.Status401Unauthorized);

            return Results.Json(unauthorized, statusCode: StatusCodes.Status401Unauthorized);
        }

        // Internal microservices (Nutrition, Smart Coach) bypass ownership checks when authenticated with service tokens
        if (currentUser.IsInternalService)
        {
            return await next(context);
        }

        Guid? targetUserId = null;

        // 1. Try route arguments
        if (context.HttpContext.Request.RouteValues.TryGetValue(_routeParamName, out var routeVal) && routeVal != null)
        {
            if (Guid.TryParse(routeVal.ToString(), out var parsedGuid))
            {
                targetUserId = parsedGuid;
            }
        }

        // 2. Try method arguments
        if (!targetUserId.HasValue)
        {
            foreach (var arg in context.Arguments)
            {
                if (arg is Guid g)
                {
                    targetUserId = g;
                    break;
                }
                if (arg is string s && Guid.TryParse(s, out var stringGuid))
                {
                    targetUserId = stringGuid;
                    break;
                }
            }
        }

        // 3. Compare with caller
        if (targetUserId.HasValue && currentUser.UserId.HasValue)
        {
            if (targetUserId.Value != currentUser.UserId.Value)
            {
                var forbidden = ApiResponse<object?>.Failure(
                    new[] { ErrorCodes.PermAccessDenied },
                    "Access denied: You can only access your own fitness records.",
                    StatusCodes.Status403Forbidden);

                return Results.Json(forbidden, statusCode: StatusCodes.Status403Forbidden);
            }
        }

        return await next(context);
    }
}
