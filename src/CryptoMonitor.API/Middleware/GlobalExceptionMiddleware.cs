using CryptoMonitor.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMonitor.API.Middleware;

internal sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var logLevel = ex is AssetNotFoundException or HttpRequestException or ValidationException
                ? LogLevel.Warning
                : LogLevel.Error;

            logger.Log(logLevel, ex, "Exception handling {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
            return;
        }

        var (statusCode, title, detail) = exception switch
        {
            AssetNotFoundException => (StatusCodes.Status404NotFound, "Asset not found", exception.Message),
            HttpRequestException => (StatusCodes.Status502BadGateway, "External service unavailable", "The upstream price provider is unavailable."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                environment.IsDevelopment() ? exception.Message : "An internal error occurred.")
        };

        context.Response.StatusCode = statusCode;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails =
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            },
            Exception = exception
        });
    }
}
