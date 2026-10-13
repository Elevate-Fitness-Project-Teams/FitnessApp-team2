using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Common.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request was cancelled by the client.");
            context.Response.StatusCode = 499; // Client Closed Request
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
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
                errors = validationException.Errors
                    .Select(e => !string.IsNullOrWhiteSpace(e.ErrorCode) && e.ErrorCode != e.PropertyName
                        ? e.ErrorCode
                        : e.ErrorMessage)
                    .Distinct()
                    .ToList();
                break;

            case NotFoundException notFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                message = notFoundException.Message;
                errors.Add(notFoundException.ErrorCode ?? ErrorCodes.FceStatsNotFound);
                break;

            case BusinessRuleException businessRuleException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = businessRuleException.Message;
                errors.Add(businessRuleException.ErrorCode ?? "BUSINESS_RULE_VIOLATION");
                break;

            case DbUpdateConcurrencyException or DbUpdateException:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "A database error occurred while persisting changes.";
                errors.Add(ErrorCodes.SrvDatabaseError);
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected server error occurred.";
                errors.Add(ErrorCodes.SrvUnexpectedError);
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object?>.Failure(errors, message, statusCode);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
