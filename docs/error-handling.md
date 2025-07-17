# Comprehensive Error Handling in Unsplasharp

This document describes the comprehensive error handling system introduced in Unsplasharp, providing structured exceptions with rich context information for better debugging and error recovery.

## Overview

The new error handling system provides:

- **Specific Exception Types**: Different exceptions for different error scenarios
- **Rich Error Context**: Detailed information about requests, responses, and timing
- **Intelligent Retry Logic**: Smart retry decisions based on error types
- **Backward Compatibility**: Existing methods continue to work as before
- **Enhanced Debugging**: Correlation IDs and structured logging

## Exception Hierarchy

### Base Exception: `UnsplasharpException`

All Unsplasharp-specific exceptions inherit from `UnsplasharpException`, which provides:

```csharp
public abstract class UnsplasharpException : Exception
{
    public string? RequestUrl { get; }           // The URL that caused the error
    public string? HttpMethod { get; }           // HTTP method used
    public ErrorContext? Context { get; }        // Rich error context
}
```

### Specific Exception Types

#### `UnsplasharpHttpException`
For HTTP-related errors with status codes:

```csharp
public class UnsplasharpHttpException : UnsplasharpException
{
    public HttpStatusCode? StatusCode { get; }   // HTTP status code
    public string? ResponseContent { get; }      // Response body
    public bool IsRetryable { get; }             // Whether error is retryable
}
```

#### `UnsplasharpAuthenticationException`
For authentication failures (401 Unauthorized):

```csharp
var client = new UnsplasharpClient("invalid_app_id");
try {
    var photo = await client.GetRandomPhotoAsync();
} catch (UnsplasharpAuthenticationException ex) {
    Console.WriteLine($"Authentication failed: {ex.Message}");
    // Check your application ID
}
```

#### `UnsplasharpNotFoundException`
For resource not found errors (404 Not Found):

```csharp
try {
    var photo = await client.GetPhotoAsync("invalid_photo_id");
} catch (UnsplasharpNotFoundException ex) {
    Console.WriteLine($"Photo '{ex.ResourceId}' not found");
    Console.WriteLine($"Resource type: {ex.ResourceType}");
}
```

#### `UnsplasharpRateLimitException`
For rate limit exceeded errors (429 Too Many Requests):

```csharp
try {
    var photos = await client.SearchPhotosAsync("nature");
} catch (UnsplasharpRateLimitException ex) {
    Console.WriteLine($"Rate limit exceeded: {ex.RateLimitRemaining}/{ex.RateLimit}");
    Console.WriteLine($"Reset time: {ex.RateLimitReset}");
    
    // Wait until reset time or implement exponential backoff
    if (ex.RateLimitReset.HasValue) {
        var waitTime = ex.RateLimitReset.Value - DateTimeOffset.UtcNow;
        await Task.Delay(waitTime);
    }
}
```

#### `UnsplasharpNetworkException`
For network-related errors:

```csharp
try {
    var photo = await client.GetRandomPhotoAsync();
} catch (UnsplasharpNetworkException ex) {
    if (ex.IsRetryable) {
        Console.WriteLine("Network error occurred, retrying...");
        // Implement retry logic
    } else {
        Console.WriteLine("Permanent network error");
    }
}
```

#### `UnsplasharpTimeoutException`
For request timeout errors:

```csharp
try {
    var photo = await client.GetRandomPhotoAsync();
} catch (UnsplasharpTimeoutException ex) {
    Console.WriteLine($"Request timed out after {ex.Timeout}");
    // Consider increasing timeout or checking network conditions
}
```

#### `UnsplasharpParsingException`
For JSON parsing errors:

```csharp
try {
    var photo = await client.GetRandomPhotoAsync();
} catch (UnsplasharpParsingException ex) {
    Console.WriteLine($"Failed to parse response: {ex.Message}");
    Console.WriteLine($"Expected type: {ex.ExpectedType}");
    // Log raw content for debugging: ex.RawContent
}
```

## Error Context

Every exception includes an `ErrorContext` object with detailed information:

```csharp
public class ErrorContext
{
    public DateTimeOffset Timestamp { get; }           // When error occurred
    public string? ApplicationId { get; }              // Your app ID
    public string? CorrelationId { get; }              // Unique request ID
    public Dictionary<string, string> RequestHeaders { get; }
    public Dictionary<string, string> ResponseHeaders { get; }
    public RateLimitInfo? RateLimitInfo { get; }       // Rate limit details
    public Dictionary<string, object> Properties { get; } // Custom properties
}
```

### Using Error Context

```csharp
try {
    var photo = await client.GetRandomPhotoAsync();
} catch (UnsplasharpException ex) {
    var context = ex.Context;
    Console.WriteLine($"Error occurred at: {context?.Timestamp}");
    Console.WriteLine($"Correlation ID: {context?.CorrelationId}");
    Console.WriteLine($"Summary: {context?.ToSummary()}");
    
    // Access rate limit information
    if (context?.RateLimitInfo != null) {
        var rateLimit = context.RateLimitInfo;
        Console.WriteLine($"Rate limit: {rateLimit.Remaining}/{rateLimit.Limit}");
    }
}
```

## Backward Compatibility

Existing methods continue to work exactly as before, returning `null` or empty collections on errors:

```csharp
// Old approach - still works
var photo = await client.GetRandomPhoto(); // Returns null on error
if (photo == null) {
    Console.WriteLine("Failed to get photo");
}

// New approach - throws exceptions
try {
    var photo = await client.GetRandomPhotoAsync(); // Throws on error
    Console.WriteLine($"Got photo: {photo.Id}");
} catch (UnsplasharpException ex) {
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Migration Guide

### For New Code
Use the new `*Async` methods that throw exceptions:

```csharp
// Instead of:
var photo = await client.GetRandomPhoto();
if (photo == null) { /* handle error */ }

// Use:
try {
    var photo = await client.GetRandomPhotoAsync();
    // Use photo
} catch (UnsplasharpException ex) {
    // Handle specific error types
}
```

### For Existing Code
No changes required - existing methods maintain their behavior:

```csharp
// This continues to work unchanged
var photos = await client.SearchPhotos("nature");
if (photos.Count == 0) {
    // Handle no results or error
}
```

### Gradual Migration
You can migrate gradually by replacing method calls one at a time:

```csharp
// Step 1: Replace method call
// var photo = await client.GetRandomPhoto();
var photo = await client.GetRandomPhotoAsync();

// Step 2: Add exception handling
try {
    var photo = await client.GetRandomPhotoAsync();
    // Use photo
} catch (UnsplasharpNotFoundException) {
    // Handle not found
} catch (UnsplasharpRateLimitException ex) {
    // Handle rate limiting
    await Task.Delay(ex.TimeUntilReset ?? TimeSpan.FromMinutes(1));
} catch (UnsplasharpException ex) {
    // Handle other errors
    logger.LogError(ex, "Unsplash API error: {Context}", ex.Context?.ToSummary());
}
```

## Best Practices

### 1. Handle Specific Exception Types
```csharp
try {
    var photo = await client.GetPhotoAsync(photoId);
} catch (UnsplasharpNotFoundException) {
    // Photo doesn't exist - show user-friendly message
} catch (UnsplasharpRateLimitException ex) {
    // Rate limited - implement backoff
    await Task.Delay(ex.TimeUntilReset ?? TimeSpan.FromMinutes(1));
} catch (UnsplasharpNetworkException ex) when (ex.IsRetryable) {
    // Transient network error - retry
} catch (UnsplasharpException ex) {
    // Other API errors - log and show generic error
    logger.LogError(ex, "API error: {Context}", ex.Context?.ToSummary());
}
```

### 2. Use Correlation IDs for Debugging
```csharp
try {
    var photo = await client.GetRandomPhotoAsync();
} catch (UnsplasharpException ex) {
    logger.LogError(ex, "Request failed [CorrelationId: {CorrelationId}]: {Message}", 
        ex.Context?.CorrelationId, ex.Message);
}
```

### 3. Implement Intelligent Retry Logic
```csharp
public async Task<Photo> GetPhotoWithRetry(string photoId, int maxRetries = 3) {
    for (int attempt = 1; attempt <= maxRetries; attempt++) {
        try {
            return await client.GetPhotoAsync(photoId);
        } catch (UnsplasharpNetworkException ex) when (ex.IsRetryable && attempt < maxRetries) {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
            await Task.Delay(delay);
        } catch (UnsplasharpRateLimitException ex) when (attempt < maxRetries) {
            var delay = ex.TimeUntilReset ?? TimeSpan.FromMinutes(1);
            await Task.Delay(delay);
        }
    }
    throw new InvalidOperationException($"Failed to get photo after {maxRetries} attempts");
}
```

### 4. Monitor Rate Limits
```csharp
try {
    var photos = await client.SearchPhotosAsync("nature");
} catch (UnsplasharpException ex) {
    if (ex.Context?.RateLimitInfo != null) {
        var rateLimit = ex.Context.RateLimitInfo;
        if (rateLimit.Remaining < 100) {
            logger.LogWarning("Rate limit running low: {Remaining}/{Limit}", 
                rateLimit.Remaining, rateLimit.Limit);
        }
    }
}
```

## Configuration

The error handling system can be configured through the retry policies:

```csharp
// The client automatically uses intelligent retry policies
var client = new UnsplasharpClient("your_app_id", logger: logger);

// Retry policies handle:
// - Exponential backoff with jitter
// - Rate limit-aware delays
// - Circuit breaker patterns (optional)
// - Comprehensive logging
```

## Logging Integration

The error handling system integrates with Microsoft.Extensions.Logging:

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var client = new UnsplasharpClient("your_app_id", logger: logger);

// All errors are automatically logged with correlation IDs and context
```

## Summary

The comprehensive error handling system provides:

- ✅ **Better Debugging**: Specific exception types with rich context
- ✅ **Intelligent Retries**: Smart retry logic based on error types  
- ✅ **Rate Limit Awareness**: Automatic rate limit detection and handling
- ✅ **Backward Compatibility**: Existing code continues to work
- ✅ **Enhanced Monitoring**: Correlation IDs and structured logging
- ✅ **Production Ready**: Circuit breakers and resilience patterns

This system makes your applications more robust and easier to debug while maintaining full backward compatibility with existing code.
