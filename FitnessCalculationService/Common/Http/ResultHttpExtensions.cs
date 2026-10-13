using FitnessCalculationService.Common.Errors;
using Microsoft.AspNetCore.Http;

namespace FitnessCalculationService.Common.Http;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        string successMessage = "Operation completed successfully.",
        int successStatusCode = StatusCodes.Status200OK,
        string? locationUri = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.Success(result.Value, successMessage, successStatusCode);
            return successStatusCode switch
            {
                StatusCodes.Status201Created => Results.Created(locationUri ?? string.Empty, response),
                _ => Results.Json(response, statusCode: successStatusCode)
            };
        }

        return MapFailureResult(result.Errors, result.Error);
    }

    public static IResult ToHttpResult(
        this Result result,
        string successMessage = "Operation completed successfully.",
        int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<object?>.Success(null, successMessage, successStatusCode);
            return Results.Json(response, statusCode: successStatusCode);
        }

        return MapFailureResult(result.Errors, result.Error);
    }

    private static IResult MapFailureResult(IReadOnlyList<Error>? errors, Error? primaryError)
    {
        var errorCode = primaryError?.Code ?? errors?.FirstOrDefault()?.Code ?? ErrorCodes.SrvUnexpectedError;
        var message = primaryError?.Message ?? errors?.FirstOrDefault()?.Message ?? "An error occurred.";
        var statusCode = ErrorHttpMapper.ToStatusCode(errorCode);

        var errorCodes = errors != null && errors.Count > 0
            ? errors.Select(e => e.Code).ToList()
            : new List<string> { errorCode };

        var failureResponse = ApiResponse<object?>.Failure(errorCodes, message, statusCode);
        return Results.Json(failureResponse, statusCode: statusCode);
    }
}
