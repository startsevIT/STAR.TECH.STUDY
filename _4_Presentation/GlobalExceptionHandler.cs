using Domain.Users;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Storage.Abstractions.Exceptions;

namespace Presentation;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception,
                "Unhandled exception for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
        EmailNotConfirmedException => (StatusCodes.Status403Forbidden, "Email not confirmed"),
        EntityNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        EntityAlreadyExistException => (StatusCodes.Status409Conflict, "Already exists"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
        InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
        _ => (StatusCodes.Status500InternalServerError, "Internal server error")
    };
}