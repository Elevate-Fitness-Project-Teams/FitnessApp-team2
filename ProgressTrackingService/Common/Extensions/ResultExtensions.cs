using Microsoft.AspNetCore.Http;
using ProgressTrackingService.Common;

namespace ProgressTrackingService.Common.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.Ok(ApiResponse<object?>.Success(null, "Operation completed successfully.", 200));

        return MapErrorToHttpResult(result.Error, result.Errors);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(ApiResponse<T>.Success(result.Value, "Operation completed successfully.", 200));

        return MapErrorToHttpResult(result.Error, result.Errors);
    }

    public static IResult ToHttpCreatedResult<T>(this Result<T> result, string uri)
    {
        if (result.IsSuccess)
            return Results.Created(uri, ApiResponse<T>.Success(result.Value, "Resource created successfully.", 201));

        return MapErrorToHttpResult(result.Error, result.Errors);
    }

    private static IResult MapErrorToHttpResult(Error error, List<Error> errors)
    {
        var errorCodesOrDescriptions = errors
            .Select(e => !string.IsNullOrWhiteSpace(e.Code) ? e.Code : e.Description)
            .Distinct()
            .ToList();

        var message = !string.IsNullOrWhiteSpace(error.Description) ? error.Description : "Operation failed.";

        return error.Type switch
        {
            ErrorType.NotFound => Results.Json(
                ApiResponse<object?>.Failure(errorCodesOrDescriptions, message, 404),
                statusCode: 404),

            ErrorType.Validation => Results.Json(
                ApiResponse<object?>.Failure(errorCodesOrDescriptions, message, 400),
                statusCode: 400),

            ErrorType.Conflict => Results.Json(
                ApiResponse<object?>.Failure(errorCodesOrDescriptions, message, 409),
                statusCode: 409),

            ErrorType.Unauthorized => Results.Json(
                ApiResponse<object?>.Failure(errorCodesOrDescriptions, message, 401),
                statusCode: 401),

            ErrorType.Forbidden => Results.Json(
                ApiResponse<object?>.Failure(errorCodesOrDescriptions, message, 403),
                statusCode: 403),

            ErrorType.Unavailable => Results.Json(
                ApiResponse<object?>.Failure(errorCodesOrDescriptions, message, 503),
                statusCode: 503),

            _ => Results.Json(
                ApiResponse<object?>.Failure(errorCodesOrDescriptions, message, 400),
                statusCode: 400)
        };
    }
}