using CryptoMonitor.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMonitor.API.Middleware;

internal sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionMiddleware> logger
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
            var logLevel = ex is AssetNotFoundException or HttpRequestException
                ? LogLevel.Warning
                : LogLevel.Error;

            logger.Log(logLevel, ex, "Exception handling {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            AssetNotFoundException => (StatusCodes.Status404NotFound, "Asset not found"),
            HttpRequestException => (StatusCodes.Status502BadGateway, "External service unavailable"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails =
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = context.Request.Path
            },
            Exception = exception
        });
    }
}
