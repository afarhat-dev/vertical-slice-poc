using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MovieLibrary.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebClientApi.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, MovieDbContext dbContext)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        // Capture request details
        var request = context.Request;
        var requestBody = await ReadRequestBodyAsync(request);
        var sourceIp = GetSourceIpAddress(context);
        var hostIp = GetHostIpAddress();

        // Create request log entry
        var requestLog = new RequestLog
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            HttpMethod = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            RequestBody = requestBody,
            SourceIpAddress = sourceIp,
            HostIpAddress = hostIp
        };

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            stopwatch.Stop();

            // Capture response details
            var responseBodyText = await ReadResponseBodyAsync(responseBody);
            requestLog.ResponseBody = responseBodyText;
            requestLog.StatusCode = context.Response.StatusCode;
            requestLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            // Log to Serilog
            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ResponseTime}ms - CorrelationId: {CorrelationId}, SourceIP: {SourceIp}",
                requestLog.HttpMethod,
                requestLog.Path,
                requestLog.StatusCode,
                requestLog.ResponseTimeMs,
                correlationId,
                sourceIp
            );

            // Save to database
            dbContext.RequestLogs.Add(requestLog);
            await dbContext.SaveChangesAsync();

            // Copy response back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log error
            requestLog.StatusCode = 500;
            requestLog.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            requestLog.ResponseBody = $"Error: {ex.Message}";

            _logger.LogError(ex,
                "HTTP {Method} {Path} failed with error in {ResponseTime}ms - CorrelationId: {CorrelationId}",
                requestLog.HttpMethod,
                requestLog.Path,
                requestLog.ResponseTimeMs,
                correlationId
            );

            // Save to database
            dbContext.RequestLogs.Add(requestLog);
            await dbContext.SaveChangesAsync();

            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(request.ContentLength ?? 0)];
        await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        request.Body.Position = 0;
        return bodyAsText;
    }

    private async Task<string> ReadResponseBodyAsync(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        return text;
    }

    private string GetSourceIpAddress(HttpContext context)
    {
        // Try to get IP from X-Forwarded-For header (for proxies/load balancers)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? "Unknown";
        }

        // Fall back to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetHostIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddress?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
