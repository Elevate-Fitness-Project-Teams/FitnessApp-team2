using FitnessCalculationService.Common.Exceptions;
using FitnessCalculationService.Common;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace FitnessCalculationService.Common.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode = (int)HttpStatusCode.InternalServerError;
        var errors = new List<string>();
        string message = "An error occurred while processing your request.";

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Validation Failed";
                errors = validationException.Errors.Select(e => $"VAL_ERROR: {e.ErrorMessage}").ToList();
                break;
            case NotFoundException notFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                message = notFoundException.Message;
                errors.Add(notFoundException.ErrorCode ?? "NOT_FOUND");
                break;
            case BusinessRuleException businessRuleException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = businessRuleException.Message;
                errors.Add(businessRuleException.ErrorCode ?? "BUSINESS_RULE_VIOLATION");
                break;
            default:
                errors.Add("INTERNAL_SERVER_ERROR");
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Failure(errors, message, statusCode);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
