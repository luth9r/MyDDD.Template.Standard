using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyDDD.Template.Application.Exceptions;

namespace MyDDD.Template.Api.Middleware;

public sealed partial class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException)
        {
            LogUnhandledExceptionMessage(logger, exception, exception.Message);
        }

        var (statusCode, title, type, detail) = exception switch
        {
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                "One or more validation errors occurred."
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                "You are not authorized to access this resource."
            ),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                "The requested resource was not found."
            ),
            IdentitySyncException => (
                StatusCodes.Status500InternalServerError,
                "Identity Sync Error",
                "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                "An error occurred while synchronizing identity data."
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server Error",
                "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                "An unexpected error occurred."
            )
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = env.IsDevelopment() ? exception.Message : detail,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException valEx)
        {
            problemDetails.Extensions["errors"] = valEx.ValidationResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
        }

        if (env.IsDevelopment())
        {
            problemDetails.Extensions["debug_exception"] = exception.ToString();
        }

        httpContext.Response.StatusCode = statusCode;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });

        return true;
    }

    [LoggerMessage(LogLevel.Error, "Unhandled exception: {Message}")]
    static partial void LogUnhandledExceptionMessage(ILogger<GlobalExceptionHandler> logger, Exception exception, string message);
}
