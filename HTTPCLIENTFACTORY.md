# IHttpClientFactory Integration Guide

This document explains how to use Unsplasharp with `IHttpClientFactory` for modern .NET applications.

## Overview

Unsplasharp now supports `IHttpClientFactory`, Microsoft's recommended pattern for managing HttpClient instances in .NET applications. This provides several benefits:

- **Proper HttpClient lifecycle management** - Automatic DNS refresh and connection pooling
- **Dependency injection integration** - Seamless integration with DI containers
- **Configuration centralization** - Configure HTTP clients in one place
- **Better testability** - Easy to mock and test HTTP interactions
- **Resource management** - Prevents socket exhaustion and DNS issues

## Quick Start

### ASP.NET Core Integration

```csharp
// Program.cs or Startup.cs
using Unsplasharp.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Unsplasharp with IHttpClientFactory
builder.Services.AddUnsplasharp("YOUR_APPLICATION_ID");

// Or with configuration
builder.Services.AddUnsplasharp(options =>
{
    options.ApplicationId = "YOUR_APPLICATION_ID";
    options.Secret = "YOUR_SECRET"; // Optional
    options.ConfigureHttpClient = client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);
        // Add custom headers, etc.
    };
});

var app = builder.Build();
```

### Using in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly UnsplasharpClient _unsplashClient;

    public PhotosController(UnsplasharpClient unsplashClient)
    {
        _unsplashClient = unsplashClient;
    }

    [HttpGet("random")]
    public async Task<IActionResult> GetRandomPhoto()
    {
        var photo = await _unsplashClient.GetRandomPhoto();
        return Ok(photo);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchPhotos(string query, int page = 1)
    {
        var photos = await _unsplashClient.SearchPhotos(query, page);
        return Ok(photos);
    }
}
```

## Manual Configuration

### Console Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Unsplasharp;
using Unsplasharp.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add Unsplasharp
        services.AddUnsplasharp("YOUR_APPLICATION_ID");
    })
    .Build();

// Use the client
var unsplashClient = host.Services.GetRequiredService<UnsplasharpClient>();
var photo = await unsplashClient.GetRandomPhoto();

Console.WriteLine($"Photo by {photo?.User?.Name}: {photo?.Urls?.Regular}");
```

### Manual DI Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Unsplasharp;

var services = new ServiceCollection();

// Add required services
services.AddHttpClient();
services.AddLogging();

// Add Unsplasharp manually
services.AddScoped<UnsplasharpClient>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var logger = provider.GetService<ILogger<UnsplasharpClient>>();
    
    return new UnsplasharpClient(
        applicationId: "YOUR_APPLICATION_ID",
        secret: null, // Optional
        logger: logger,
        httpClientFactory: httpClientFactory
    );
});

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<UnsplasharpClient>();
```

## Advanced Configuration

### Custom HttpClient Configuration

```csharp
services.AddUnsplasharp(options =>
{
    options.ApplicationId = "YOUR_APPLICATION_ID";
    options.ConfigureHttpClient = client =>
    {
        // Custom timeout
        client.Timeout = TimeSpan.FromMinutes(2);
        
        // Custom headers
        client.DefaultRequestHeaders.Add("X-Custom-Header", "MyValue");
        
        // Custom user agent
        client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0");
    };
});
```

### Multiple Clients with Different Configurations

```csharp
// Register multiple named clients
services.AddHttpClient("unsplash-primary", client =>
{
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Client-ID", "PRIMARY_APP_ID");
    client.Timeout = TimeSpan.FromSeconds(30);
});

services.AddHttpClient("unsplash-secondary", client =>
{
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Client-ID", "SECONDARY_APP_ID");
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Register clients manually
services.AddScoped<UnsplasharpClient>("primary", provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var logger = provider.GetService<ILogger<UnsplasharpClient>>();
    return new UnsplasharpClient("PRIMARY_APP_ID", logger: logger, httpClientFactory: factory);
});
```

### Configuration from appsettings.json

```json
{
  "Unsplash": {
    "ApplicationId": "YOUR_APPLICATION_ID",
    "Secret": "YOUR_SECRET",
    "Timeout": "00:01:00"
  }
}
```

```csharp
// Configuration class
public class UnsplashOptions
{
    public string ApplicationId { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}

// In Program.cs
var unsplashOptions = builder.Configuration.GetSection("Unsplash").Get<UnsplashOptions>();

builder.Services.AddUnsplasharp(options =>
{
    options.ApplicationId = unsplashOptions.ApplicationId;
    options.Secret = unsplashOptions.Secret;
    options.ConfigureHttpClient = client =>
    {
        client.Timeout = unsplashOptions.Timeout;
    };
});
```

## Backward Compatibility

The library maintains full backward compatibility. Existing code will continue to work:

```csharp
// Legacy usage (still works)
var client = new UnsplasharpClient("YOUR_APPLICATION_ID");
var photos = await client.SearchPhotos("nature");

// With logging (still works)
var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var client = new UnsplasharpClient("YOUR_APPLICATION_ID", logger: logger);
```

## Benefits of IHttpClientFactory

### Automatic Resource Management
- Prevents socket exhaustion
- Handles DNS changes automatically
- Manages connection pooling

### Better Performance
- Reuses connections efficiently
- Reduces memory allocations
- Optimizes network usage

### Enhanced Reliability
- Built-in retry policies (when configured)
- Better error handling
- Improved timeout management

### Improved Testability
```csharp
// Easy to mock for testing
var mockFactory = new Mock<IHttpClientFactory>();
var mockHttpClient = new Mock<HttpClient>();
mockFactory.Setup(f => f.CreateClient("unsplash")).Returns(mockHttpClient.Object);

var client = new UnsplasharpClient("test", httpClientFactory: mockFactory.Object);
```

## Migration Guide

### From Legacy to IHttpClientFactory

1. **Add the extension method**:
   ```csharp
   // Old
   services.AddScoped(_ => new UnsplasharpClient("APP_ID"));
   
   // New
   services.AddUnsplasharp("APP_ID");
   ```

2. **Update constructor calls**:
   ```csharp
   // Old
   var client = new UnsplasharpClient("APP_ID");
   
   // New (in DI context)
   // Inject UnsplasharpClient directly
   ```

3. **Configuration changes**:
   ```csharp
   // Old - manual HttpClient management
   // No centralized configuration
   
   // New - centralized configuration
   services.AddUnsplasharp(options =>
   {
       options.ApplicationId = "APP_ID";
       options.ConfigureHttpClient = client => { /* configure */ };
   });
   ```

## Troubleshooting

### Common Issues

1. **Missing HttpClient registration**: Ensure `AddHttpClient()` is called
2. **DI scope issues**: Register as Scoped or Transient, not Singleton
3. **Configuration not applied**: Check that ConfigureHttpClient is called correctly

### Debug Logging

Enable debug logging to see HTTP client creation:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});
```

This will show when HttpClients are created and reused.
