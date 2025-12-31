using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace WebClientApi.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from header, or generate a new one
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Store in HttpContext items for easy access
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        // Add to Serilog LogContext
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
