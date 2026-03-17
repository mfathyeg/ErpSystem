using System.Net;
using System.Text.Json;
using ErpSystem.SharedKernel.Exceptions;
using FluentValidation;

namespace ErpSystem.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogError(
            exception,
            "An error occurred. CorrelationId: {CorrelationId}",
            correlationId);

        var (statusCode, response) = exception switch
        {
            EntityNotFoundException ex => (
                HttpStatusCode.NotFound,
                new ErrorResponse(ex.Code, ex.Message, correlationId)),

            BusinessRuleValidationException ex => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(ex.Code, ex.Message, correlationId)),

            SharedKernel.Exceptions.ValidationException ex => (
                HttpStatusCode.BadRequest,
                new ValidationErrorResponse("Validation.Failed", "Validation failed", correlationId, ex.Errors)),

            FluentValidation.ValidationException ex => (
                HttpStatusCode.BadRequest,
                new ValidationErrorResponse(
                    "Validation.Failed",
                    "Validation failed",
                    correlationId,
                    ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))),

            ConcurrencyException ex => (
                HttpStatusCode.Conflict,
                new ErrorResponse(ex.Code, ex.Message, correlationId)),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse("Unauthorized", "Unauthorized access", correlationId)),

            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("InternalError", "An unexpected error occurred", correlationId))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

public record ErrorResponse(string Code, string Message, string CorrelationId);

public record ValidationErrorResponse(
    string Code,
    string Message,
    string CorrelationId,
    IReadOnlyDictionary<string, string[]> Errors) : ErrorResponse(Code, Message, CorrelationId);
