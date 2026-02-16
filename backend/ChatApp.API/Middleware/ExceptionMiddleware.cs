using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using ChatApp.Application.Common.Exceptions;
using ChatApp.Domain.Exceptions;

namespace ChatApp.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                (object?)ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })),
            NotFoundException ne => (
                StatusCodes.Status404NotFound,
                ne.Message,
                null),
            UserAlreadyExistsException ue => (
                StatusCodes.Status409Conflict,
                ue.Message,
                null),
            PrivateChatException pe => (
                StatusCodes.Status400BadRequest,
                pe.Message,
                null),
            ForbiddenAccessException fe => (
                StatusCodes.Status403Forbidden,
                fe.Message,
                null),
            DomainException de => (
                StatusCodes.Status400BadRequest,
                de.Message,
                null),
            UnauthorizedAccessException ua => (
                StatusCodes.Status401Unauthorized,
                string.IsNullOrWhiteSpace(ua.Message) ? "Unauthorized" : ua.Message,
                null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                null)
        };

        Console.WriteLine($"[ERROR {statusCode}]: {exception.GetType().Name} — {exception.Message}");
        if (statusCode == StatusCodes.Status500InternalServerError)
            Console.WriteLine($"[STACKTRACE]: {exception.StackTrace}");

        context.Response.StatusCode = statusCode;

        var response = new { status = statusCode, message, errors };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}