using ProgressTrackingService.Common;

namespace ProgressTrackingService.Common.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.Ok(ApiResponse<object>.Success(null!));

        return MapErrorToHttpResult(result.Error, result.Errors);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(ApiResponse<T>.Success(result.Value));

        return MapErrorToHttpResult(result.Error, result.Errors);
    }

    private static IResult MapErrorToHttpResult(Error error, List<Error> errors)
    {
        var errorMessages = errors.Select(e => e.Description);

        return error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(
                ApiResponse<object>.Failure(errorMessages, "Resource not found.")),

            ErrorType.Validation => Results.BadRequest(
                ApiResponse<object>.Failure(errorMessages, "Validation failed.")),

            ErrorType.Conflict => Results.Conflict(
                ApiResponse<object>.Failure(errorMessages, "Conflict.")),

            ErrorType.Unauthorized => Results.Json(
                ApiResponse<object>.Failure(errorMessages, "Unauthorized."),
                statusCode: 401),

            ErrorType.Forbidden => Results.Json(
                ApiResponse<object>.Failure(errorMessages, "Forbidden."),
                statusCode: 403),

            _ => Results.BadRequest(
                ApiResponse<object>.Failure(errorMessages))
        };
    }
}