using System;

namespace MovieLibrary.Data;

public class RequestLog
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public string RequestBody { get; set; } = string.Empty;
    public string SourceIpAddress { get; set; } = string.Empty;
    public string HostIpAddress { get; set; } = string.Empty;
    public string? ResponseBody { get; set; }
    public int? StatusCode { get; set; }
    public long? ResponseTimeMs { get; set; }
}
