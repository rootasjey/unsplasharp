# Logging Guide

This document explains how to use the structured logging features in Unsplasharp.

## Overview

Unsplasharp now includes built-in support for Microsoft.Extensions.Logging, providing detailed insights into:
- HTTP requests and responses
- Retry attempts and failures
- Rate limit information
- Error conditions and exceptions

## Quick Start

### Basic Setup

```csharp
using Microsoft.Extensions.Logging;
using Unsplasharp;

// Create a logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));

var logger = loggerFactory.CreateLogger<UnsplasharpClient>();

// Create client with logging
var client = new UnsplasharpClient("YOUR_APP_ID", logger: logger);
```

### ASP.NET Core Integration

```csharp
// In Startup.cs or Program.cs
services.AddScoped<UnsplasharpClient>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<UnsplasharpClient>>();
    return new UnsplasharpClient("YOUR_APP_ID", logger: logger);
});
```

## Log Levels

### Debug Level
- HTTP request URLs
- HttpClient creation events
- Rate limit status after successful requests

```
[Debug] Making HTTP request to https://api.unsplash.com/photos/random
[Debug] Created new HttpClient for application ID your-app-id
[Debug] HTTP request successful. Rate limit: 4999/5000
```

### Information Level
- Search operations with parameters and results
- Photo retrieval operations
- High-level operation outcomes

```
[Information] Searching photos with query 'nature', page 1, perPage 10
[Information] Photo search completed. Found 10 photos, total results: 1500
[Information] Fetching random photo
[Information] Successfully retrieved random photo with ID abc123
```

### Warning Level
- Retry attempts due to transient failures
- Timeout scenarios
- Failed photo retrievals

```
[Warning] Retrying HTTP request (attempt 2/3). Exception: The operation was canceled.
[Warning] HTTP request timed out for URL https://api.unsplash.com/photos/random after retries
[Warning] Failed to retrieve random photo
```

### Error Level
- HTTP request failures after all retries
- Unexpected exceptions
- Network-related errors

```
[Error] HTTP request failed for URL https://api.unsplash.com/photos/random after retries
[Error] Unexpected error during HTTP request to https://api.unsplash.com/search/photos after retries
```

## Configuration Examples

### Console Logging with Custom Format

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    })
    .SetMinimumLevel(LogLevel.Debug));
```

### File Logging with Serilog

```csharp
using Serilog;
using Microsoft.Extensions.Logging;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("unsplasharp.log")
    .CreateLogger();

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddSerilog());

var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var client = new UnsplasharpClient("YOUR_APP_ID", logger: logger);
```

### Structured Logging with Properties

The library uses structured logging with named properties:

```csharp
// Example log entries with structured properties
_logger.LogInformation("Searching photos with query '{Query}', page {Page}, perPage {PerPage}", 
    query, page, perPage);

_logger.LogWarning("Retrying HTTP request (attempt {AttemptNumber}/{MaxAttempts}). Exception: {Exception}", 
    attemptNumber, maxAttempts, exception.Message);
```

## Filtering Logs

### By Category

```csharp
builder.AddFilter("Unsplasharp.UnsplasharpClient", LogLevel.Information);
```

### By Log Level

```csharp
builder.SetMinimumLevel(LogLevel.Warning); // Only warnings and errors
```

### Custom Filtering

```csharp
builder.AddFilter((category, level) =>
    category.Contains("Unsplasharp") && level >= LogLevel.Information);
```

## Monitoring and Observability

### Application Insights Integration

```csharp
services.AddApplicationInsightsTelemetry();
services.AddLogging(builder => builder.AddApplicationInsights());
```

### Custom Log Processors

```csharp
public class UnsplasharpLogProcessor : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        if (categoryName.Contains("UnsplasharpClient"))
        {
            return new CustomUnsplasharpLogger();
        }
        return NullLogger.Instance;
    }
    
    public void Dispose() { }
}
```

## Performance Considerations

- Logging is designed to have minimal performance impact
- Debug-level logging may affect performance in high-throughput scenarios
- Consider using Information level or higher in production
- Structured logging properties are efficiently formatted

## Troubleshooting

### Common Issues

1. **No logs appearing**: Check log level configuration
2. **Too many logs**: Increase minimum log level
3. **Missing retry logs**: Ensure Warning level is enabled

### Debug Mode

Enable debug logging to see detailed HTTP request information:

```csharp
builder.SetMinimumLevel(LogLevel.Debug);
```

This will show:
- Exact URLs being requested
- HttpClient lifecycle events
- Rate limit information
- Detailed retry attempt information
