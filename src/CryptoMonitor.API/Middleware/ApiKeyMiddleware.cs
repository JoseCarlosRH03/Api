using CryptoMonitor.Application.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CryptoMonitor.API.Middleware;

internal sealed class ApiKeyMiddleware(
    RequestDelegate next,
    IOptions<ApiSecurityOptions> options
)
{
    private const string ApiKeyHeader = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey)
            || providedKey != options.Value.ApiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "A valid API key must be provided in the X-Api-Key header."
            });
            return;
        }

        await next(context);
    }
}
