# Migration and Upgrade Guide

This guide helps you migrate from older versions of Unsplasharp and adopt new features like comprehensive error handling, IHttpClientFactory integration, and modern async patterns.

## Table of Contents

- [Version Compatibility](#version-compatibility)
- [Breaking Changes](#breaking-changes)
- [New Features Overview](#new-features-overview)
- [Migration Strategies](#migration-strategies)
- [Error Handling Migration](#error-handling-migration)
- [IHttpClientFactory Migration](#ihttpclientfactory-migration)
- [Logging Integration](#logging-integration)
- [Performance Improvements](#performance-improvements)
- [Best Practices Updates](#best-practices-updates)

## Version Compatibility

### Supported .NET Versions

| Unsplasharp Version | .NET Framework | .NET Core | .NET 5+ |
|-------------------|----------------|-----------|---------|
| 1.x               | 4.6.1+         | 2.0+      | ❌      |
| 2.x               | 4.6.1+         | 2.0+      | 5.0+    |
| 3.x (Current)     | 4.6.1+         | 2.0+      | 5.0+    |

### API Compatibility

- **Backward Compatible**: All existing methods continue to work
- **New Methods**: Exception-throwing variants added (e.g., `GetPhotoAsync`)
- **Enhanced Features**: Logging, metrics, and error context added
- **No Breaking Changes**: Existing code will compile and run without modifications

## Breaking Changes

### None! 🎉

Unsplasharp maintains full backward compatibility. Your existing code will continue to work exactly as before.

```csharp
// This code from v1.x still works in v3.x
var client = new UnsplasharpClient("YOUR_APP_ID");
var photo = await client.GetRandomPhoto();
if (photo != null)
{
    Console.WriteLine(photo.Urls.Regular);
}
```

## New Features Overview

### 1. Comprehensive Error Handling

- **New Exception Types**: Specific exceptions for different error scenarios
- **Rich Error Context**: Detailed information about requests and responses
- **Correlation IDs**: For better debugging and monitoring
- **Rate Limit Awareness**: Automatic rate limit detection and handling

### 2. IHttpClientFactory Integration

- **Modern HTTP Management**: Proper HttpClient lifecycle management
- **Dependency Injection**: Seamless integration with DI containers
- **Connection Pooling**: Better performance and resource utilization
- **Configuration Centralization**: Configure HTTP clients in one place

### 3. Structured Logging

- **Microsoft.Extensions.Logging**: Integration with standard logging framework
- **Detailed Insights**: HTTP requests, retries, rate limits, and errors
- **Correlation Tracking**: Link related log entries with correlation IDs
- **Performance Metrics**: Request timing and success rates

### 4. Enhanced Async Support

- **CancellationToken Support**: All methods now support cancellation
- **Better Exception Handling**: Exception-throwing variants for better error handling
- **Timeout Management**: Configurable timeouts with proper cancellation

## Migration Strategies

### Strategy 1: Gradual Migration (Recommended)

Migrate your codebase gradually by replacing method calls one at a time:

```csharp
// Step 1: Keep existing code working
var client = new UnsplasharpClient("YOUR_APP_ID");
var photo = await client.GetRandomPhoto(); // Old method

// Step 2: Add logging (optional)
var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var clientWithLogging = new UnsplasharpClient("YOUR_APP_ID", logger: logger);

// Step 3: Replace with exception-throwing methods
try
{
    var photo = await client.GetRandomPhotoAsync(); // New method
    // Handle success
}
catch (UnsplasharpException ex)
{
    // Handle errors
}

// Step 4: Add IHttpClientFactory (for new projects or major refactoring)
services.AddUnsplasharp("YOUR_APP_ID");
```

### Strategy 2: New Project Setup

For new projects, start with the modern approach:

```csharp
// Program.cs
services.AddUnsplasharp(options =>
{
    options.ApplicationId = configuration["Unsplash:ApplicationId"];
    options.ConfigureHttpClient = client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    };
});

// Service class
public class PhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(UnsplasharpClient client, ILogger<PhotoService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Photo?> GetPhotoAsync(string photoId)
    {
        try
        {
            return await _client.GetPhotoAsync(photoId);
        }
        catch (UnsplasharpNotFoundException)
        {
            _logger.LogWarning("Photo {PhotoId} not found", photoId);
            return null;
        }
        catch (UnsplasharpException ex)
        {
            _logger.LogError(ex, "Error getting photo {PhotoId}", photoId);
            throw;
        }
    }
}
```

## Error Handling Migration

### Before: Basic Error Handling

```csharp
// Old approach - null checking
var photo = await client.GetRandomPhoto();
if (photo == null)
{
    Console.WriteLine("Failed to get photo");
    return;
}

// Process photo
Console.WriteLine($"Photo by {photo.User.Name}");
```

### After: Comprehensive Error Handling

```csharp
// New approach - exception handling
try
{
    var photo = await client.GetRandomPhotoAsync();
    Console.WriteLine($"Photo by {photo.User.Name}");
}
catch (UnsplasharpNotFoundException)
{
    Console.WriteLine("Photo not found");
}
catch (UnsplasharpRateLimitException ex)
{
    Console.WriteLine($"Rate limited. Reset at: {ex.RateLimitReset}");
    // Implement backoff strategy
}
catch (UnsplasharpAuthenticationException)
{
    Console.WriteLine("Invalid API key");
}
catch (UnsplasharpNetworkException ex) when (ex.IsRetryable)
{
    Console.WriteLine("Network error - retrying...");
    // Implement retry logic
}
catch (UnsplasharpException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
    
    // Access rich error context
    if (ex.Context != null)
    {
        Console.WriteLine($"Correlation ID: {ex.Context.CorrelationId}");
        Console.WriteLine($"Request URL: {ex.RequestUrl}");
    }
}
```

### Migration Helper

```csharp
public static class MigrationHelper
{
    /// <summary>
    /// Wraps old-style methods to provide exception-based error handling
    /// </summary>
    public static async Task<T> WrapWithExceptions<T>(Func<Task<T?>> oldMethod) where T : class
    {
        var result = await oldMethod();
        if (result == null)
        {
            throw new InvalidOperationException("Operation returned null - this may indicate an API error");
        }
        return result;
    }
}

// Usage
try
{
    var photo = await MigrationHelper.WrapWithExceptions(() => client.GetRandomPhoto());
    // Process photo
}
catch (Exception ex)
{
    // Handle error
}
```

## IHttpClientFactory Migration

### Before: Manual HttpClient Management

```csharp
// Old approach - basic client creation
var client = new UnsplasharpClient("YOUR_APP_ID");

// Or with logging
var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var client = new UnsplasharpClient("YOUR_APP_ID", logger: logger);
```

### After: IHttpClientFactory Integration

```csharp
// New approach - dependency injection setup
// In Program.cs or Startup.cs
services.AddUnsplasharp("YOUR_APP_ID");

// Or with configuration
services.AddUnsplasharp(options =>
{
    options.ApplicationId = configuration["Unsplash:ApplicationId"];
    options.Secret = configuration["Unsplash:Secret"];
    options.ConfigureHttpClient = client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0");
    };
});

// In your service
public class PhotoService
{
    private readonly UnsplasharpClient _client;

    public PhotoService(UnsplasharpClient client)
    {
        _client = client; // Injected with proper HttpClient management
    }
}
```

### Manual IHttpClientFactory Setup

If you can't use the extension method:

```csharp
// Manual setup
services.AddHttpClient();
services.AddScoped<UnsplasharpClient>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var logger = provider.GetService<ILogger<UnsplasharpClient>>();
    
    return new UnsplasharpClient(
        applicationId: "YOUR_APP_ID",
        logger: logger,
        httpClientFactory: httpClientFactory
    );
});
```

## Logging Integration

### Before: No Logging

```csharp
var client = new UnsplasharpClient("YOUR_APP_ID");
var photo = await client.GetRandomPhoto();
// No visibility into what's happening
```

### After: Structured Logging

```csharp
// Setup logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Client with logging
var logger = serviceProvider.GetRequiredService<ILogger<UnsplasharpClient>>();
var client = new UnsplasharpClient("YOUR_APP_ID", logger: logger);

// Or with DI
services.AddUnsplasharp("YOUR_APP_ID"); // Automatically includes logging

// Now you get detailed logs:
// [Information] Making HTTP request to https://api.unsplash.com/photos/random
// [Debug] Rate limit: 4999/5000
// [Information] Request completed in 245ms
```

### Custom Logging Configuration

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    });
    
    // Set specific log levels
    builder.AddFilter("Unsplasharp", LogLevel.Information);
    builder.AddFilter("System.Net.Http", LogLevel.Warning);
});
```

## Performance Improvements

### Connection Pooling

The new version automatically uses connection pooling when IHttpClientFactory is available:

```csharp
// Old: Each client instance creates its own HttpClient
var client1 = new UnsplasharpClient("APP_ID");
var client2 = new UnsplasharpClient("APP_ID"); // Creates another HttpClient

// New: Shared connection pool
services.AddUnsplasharp("APP_ID");
// All injected clients share the same optimized HttpClient pool
```

### Retry Policies

Built-in retry policies with exponential backoff:

```csharp
// Automatic retry for transient failures
try
{
    var photo = await client.GetPhotoAsync("photo-id");
}
catch (UnsplasharpNetworkException ex) when (ex.IsRetryable)
{
    // The client already attempted retries with exponential backoff
    // This exception means all retries were exhausted
}
```

### Rate Limit Optimization

Better rate limit handling:

```csharp
// Automatic rate limit tracking
Console.WriteLine($"Rate limit: {client.RateLimitRemaining}/{client.MaxRateLimit}");

// Smart retry on rate limit exceeded
try
{
    var photos = await client.SearchPhotosAsync("nature");
}
catch (UnsplasharpRateLimitException ex)
{
    // Exception includes exact reset time
    var waitTime = ex.RateLimitReset - DateTimeOffset.UtcNow;
    await Task.Delay(waitTime);
    // Retry the request
}
```

## Best Practices Updates

### 1. Use CancellationTokens

All methods now support cancellation tokens:

```csharp
// Before: No cancellation support
var photos = await client.SearchPhotos("nature");

// After: With cancellation support
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
try
{
    var photos = await client.SearchPhotosAsync("nature", cancellationToken: cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Search timed out");
}
```

### 2. Implement Proper Error Handling

```csharp
// Before: Basic null checking
public async Task<List<Photo>> GetPhotosOldWay(string query)
{
    var photos = await client.SearchPhotos(query);
    return photos ?? new List<Photo>();
}

// After: Comprehensive error handling
public async Task<List<Photo>> GetPhotosNewWay(string query)
{
    try
    {
        return await client.SearchPhotosAsync(query);
    }
    catch (UnsplasharpNotFoundException)
    {
        return new List<Photo>();
    }
    catch (UnsplasharpRateLimitException ex)
    {
        logger.LogWarning("Rate limited, waiting {Delay}ms", ex.TimeUntilReset?.TotalMilliseconds);
        throw; // Let caller handle rate limiting
    }
    catch (UnsplasharpException ex)
    {
        logger.LogError(ex, "Search failed for query: {Query}", query);
        throw;
    }
}
```

### 3. Use Dependency Injection

```csharp
// Before: Manual instantiation
public class PhotoService
{
    private readonly UnsplasharpClient _client;

    public PhotoService()
    {
        _client = new UnsplasharpClient("YOUR_APP_ID");
    }
}

// After: Dependency injection
public class PhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(UnsplasharpClient client, ILogger<PhotoService> logger)
    {
        _client = client;
        _logger = logger;
    }
}

// Registration
services.AddUnsplasharp("YOUR_APP_ID");
services.AddScoped<PhotoService>();
```

### 4. Implement Caching

```csharp
// Enhanced caching with error handling
public class CachedPhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedPhotoService> _logger;

    public async Task<Photo?> GetPhotoAsync(string photoId)
    {
        var cacheKey = $"photo:{photoId}";

        if (_cache.TryGetValue(cacheKey, out Photo cachedPhoto))
        {
            return cachedPhoto;
        }

        try
        {
            var photo = await _client.GetPhotoAsync(photoId);
            _cache.Set(cacheKey, photo, TimeSpan.FromHours(1));
            return photo;
        }
        catch (UnsplasharpNotFoundException)
        {
            // Cache negative results for shorter time
            _cache.Set(cacheKey, (Photo?)null, TimeSpan.FromMinutes(5));
            return null;
        }
        catch (UnsplasharpRateLimitException)
        {
            // Don't cache rate limit errors
            throw;
        }
        catch (UnsplasharpException ex)
        {
            _logger.LogError(ex, "Failed to get photo {PhotoId}", photoId);
            throw;
        }
    }
}
```

## Troubleshooting Common Migration Issues

### Issue 1: Null Reference Exceptions

**Problem**: Code that worked before now throws null reference exceptions.

**Cause**: You might be using new exception-throwing methods without proper error handling.

**Solution**:
```csharp
// If you get NullReferenceException here:
var photo = await client.GetPhotoAsync("invalid-id");
Console.WriteLine(photo.Description); // NullReferenceException

// Change to:
try
{
    var photo = await client.GetPhotoAsync("invalid-id");
    Console.WriteLine(photo.Description);
}
catch (UnsplasharpNotFoundException)
{
    Console.WriteLine("Photo not found");
}
```

### Issue 2: HttpClient Disposal Issues

**Problem**: "Cannot access a disposed object" errors.

**Cause**: Manual HttpClient management conflicts with IHttpClientFactory.

**Solution**:
```csharp
// Don't do this:
using var httpClient = new HttpClient();
var client = new UnsplasharpClient("APP_ID", httpClientFactory: someFactory);

// Do this instead:
services.AddUnsplasharp("APP_ID"); // Let DI handle lifecycle
```

### Issue 3: Rate Limit Handling

**Problem**: Application stops working when rate limits are hit.

**Solution**:
```csharp
public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (UnsplasharpRateLimitException ex) when (attempt < maxRetries)
        {
            var delay = ex.TimeUntilReset ?? TimeSpan.FromMinutes(1);
            await Task.Delay(delay);
        }
        catch (UnsplasharpNetworkException ex) when (ex.IsRetryable && attempt < maxRetries)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            await Task.Delay(delay);
        }
    }

    throw new InvalidOperationException($"Operation failed after {maxRetries} attempts");
}

// Usage
var photo = await ExecuteWithRetry(() => client.GetPhotoAsync("photo-id"));
```

### Issue 4: Configuration Problems

**Problem**: API key not being recognized or configuration not loading.

**Solution**:
```csharp
// Check configuration loading
var config = builder.Configuration.GetSection("Unsplash").Get<UnsplashConfiguration>();
if (string.IsNullOrEmpty(config?.ApplicationId))
{
    throw new InvalidOperationException("Unsplash ApplicationId not configured");
}

// Validate at startup
services.AddUnsplasharp(options =>
{
    options.ApplicationId = config.ApplicationId ??
        throw new ArgumentNullException(nameof(config.ApplicationId));
});
```

## Migration Checklist

### Phase 1: Preparation
- [ ] Review current Unsplasharp usage in your codebase
- [ ] Identify error handling patterns
- [ ] Plan migration strategy (gradual vs. complete)
- [ ] Set up logging infrastructure
- [ ] Update NuGet package

### Phase 2: Basic Migration
- [ ] Add logging to existing clients
- [ ] Replace null checks with try-catch blocks
- [ ] Add CancellationToken support to async methods
- [ ] Test existing functionality

### Phase 3: Advanced Features
- [ ] Implement IHttpClientFactory integration
- [ ] Add comprehensive error handling
- [ ] Implement retry policies
- [ ] Add performance monitoring
- [ ] Update caching strategies

### Phase 4: Optimization
- [ ] Review and optimize HTTP client configuration
- [ ] Implement connection pooling
- [ ] Add metrics collection
- [ ] Performance testing
- [ ] Documentation updates

## Testing Your Migration

### Unit Testing

```csharp
[Test]
public async Task GetPhoto_WithValidId_ReturnsPhoto()
{
    // Arrange
    var mockFactory = new Mock<IHttpClientFactory>();
    var mockLogger = new Mock<ILogger<UnsplasharpClient>>();
    var client = new UnsplasharpClient("test-app-id", logger: mockLogger.Object, httpClientFactory: mockFactory.Object);

    // Act & Assert
    try
    {
        var photo = await client.GetPhotoAsync("valid-photo-id");
        Assert.IsNotNull(photo);
    }
    catch (UnsplasharpNotFoundException)
    {
        // Expected for invalid test ID
        Assert.Pass("Exception handling working correctly");
    }
}

[Test]
public async Task GetPhoto_WithInvalidId_ThrowsNotFoundException()
{
    var client = new UnsplasharpClient("test-app-id");

    await Assert.ThrowsAsync<UnsplasharpNotFoundException>(
        () => client.GetPhotoAsync("invalid-photo-id"));
}
```

### Integration Testing

```csharp
[Test]
public async Task Integration_SearchPhotos_ReturnsResults()
{
    var client = new UnsplasharpClient(TestConfiguration.ApplicationId);

    var photos = await client.SearchPhotosAsync("nature", perPage: 5);

    Assert.IsNotEmpty(photos);
    Assert.All(photos, photo => Assert.IsNotNull(photo.Id));
}
```

## Performance Comparison

### Before Migration
- Manual HttpClient management
- No connection pooling
- Basic error handling
- No retry logic
- No structured logging

### After Migration
- Optimized HttpClient with connection pooling
- Automatic retry with exponential backoff
- Comprehensive error handling with context
- Structured logging with correlation IDs
- Rate limit awareness

### Expected Improvements
- **Reduced memory usage**: Better HttpClient lifecycle management
- **Improved reliability**: Automatic retries and better error handling
- **Better observability**: Structured logging and metrics
- **Enhanced performance**: Connection pooling and optimized HTTP settings

---

## Summary

The migration to the latest Unsplasharp version provides significant improvements while maintaining full backward compatibility. Key benefits include:

✅ **Zero Breaking Changes** - Existing code continues to work
✅ **Enhanced Error Handling** - Specific exceptions with rich context
✅ **Modern HTTP Management** - IHttpClientFactory integration
✅ **Structured Logging** - Better observability and debugging
✅ **Improved Performance** - Connection pooling and retry policies
✅ **Better Testing** - Easier to mock and test

Take your time with the migration and adopt new features gradually. The investment in proper error handling and logging will pay dividends in production reliability and maintainability.
```
