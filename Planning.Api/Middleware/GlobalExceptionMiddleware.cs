using System.Net;
using System.Text.Json;

namespace Planning.Api.Middleware;

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
            _logger.LogError(ex, "Unhandled exception occurred.");
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var payload = new
        {
            error = "An unexpected error occurred.",
            errorCode = "INTERNAL_ERROR",
            traceId = context.TraceIdentifier,
            detail = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true
                ? exception.Message
                : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
