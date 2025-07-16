# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased] - 2025-01-16

### Added
- **Cancellation Token Support**: Comprehensive `CancellationToken` support for all async methods
  - All public async methods now have `CancellationToken` overloads
  - Proper cancellation token propagation through HTTP layer and retry policies
  - Request cancellation and timeout support
  - Exception handling distinguishes user cancellation from timeouts
  - 100% backward compatible - existing code works unchanged
  - Enables modern async patterns and ASP.NET Core integration

- **Structured Logging Support**: Added Microsoft.Extensions.Logging integration
  - Optional `ILogger<UnsplasharpClient>` parameter in constructor
  - Detailed logging of HTTP requests, responses, and errors
  - Rate limit information logging
  - Retry attempt logging with context

- **Retry Policies**: Implemented resilient HTTP requests using Polly
  - Automatic retry for `HttpRequestException` and `TaskCanceledException`
  - Exponential backoff strategy (1s, 2s, 4s delays)
  - Maximum of 3 retry attempts
  - Structured logging of retry attempts

- **IHttpClientFactory Integration**: Modern HTTP client management pattern
  - Optional `IHttpClientFactory` parameter in constructor
  - Service collection extension methods for easy DI registration
  - Named HttpClient configuration with proper headers and policies
  - Backward compatible with existing HttpClient management
  - Better resource management and connection pooling

- **Enhanced Async/Await Patterns**
  - Added `ConfigureAwait(false)` to all async calls for better performance
  - Improved exception handling in async methods
  - Better resource management and disposal patterns

- **System.Text.Json Migration**: Migrated from Newtonsoft.Json to System.Text.Json
  - Significant performance improvements in JSON parsing and serialization
  - Reduced memory allocations and faster processing
  - Better integration with modern .NET ecosystem
  - Custom JsonConverter classes for complex model serialization
  - JsonPropertyName attributes for proper property mapping
  - Comprehensive JsonHelper extension methods for safe property access

### Changed
- **Target Framework**: Updated from .NET Standard 1.4 to .NET Standard 2.0
- **Dependencies**:
  - Added `Microsoft.Extensions.Logging.Abstractions` 8.0.0
  - Added `Microsoft.Extensions.Http` 8.0.0
  - Added `Polly` 8.2.0
  - **Removed** `Newtonsoft.Json` (replaced with System.Text.Json)
  - System.Text.Json is included in .NET Standard 2.0+ runtime
- **Constructor**: Enhanced `UnsplasharpClient` constructor with optional logger parameter
- **HTTP Client Management**: Improved HttpClient lifecycle management with better logging

### Improved
- **Error Handling**: Enhanced exception handling with more specific error messages
- **Performance**: Better async patterns and resource management
- **Reliability**: Automatic retry logic for transient failures
- **Observability**: Comprehensive logging for debugging and monitoring
- **Code Quality**: Better null handling and defensive programming practices

### Technical Details

#### Logging Integration
```csharp
// Basic usage (no logging)
var client = new UnsplasharpClient("YOUR_APP_ID");

// With logging
var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var client = new UnsplasharpClient("YOUR_APP_ID", logger: logger);
```

#### Retry Policy Configuration
- **Handled Exceptions**: `HttpRequestException`, `TaskCanceledException`
- **Max Attempts**: 3 retries
- **Backoff**: Exponential (1s → 2s → 4s)
- **Logging**: Each retry attempt is logged with context

#### Async Improvements
- All async methods now use `ConfigureAwait(false)`
- Better exception propagation
- Improved cancellation token support
- Enhanced resource disposal patterns

### Breaking Changes
- **Target Framework**: Minimum requirement changed from .NET Standard 1.4 to .NET Standard 2.0
- **Constructor**: Added optional parameters (backward compatible)

### Migration Guide

#### From Previous Versions
```csharp
// Old usage (still works)
var client = new UnsplasharpClient("YOUR_APP_ID");

// New usage with logging (optional)
var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var client = new UnsplasharpClient("YOUR_APP_ID", logger: logger);
```

#### Dependencies
If you're upgrading, you may need to update your project's target framework:
```xml
<TargetFramework>netstandard2.0</TargetFramework>
<!-- or -->
<TargetFramework>net6.0</TargetFramework>
```

### Performance Improvements
- **System.Text.Json Migration**: Up to 2-3x faster JSON parsing compared to Newtonsoft.Json
- Reduced memory allocations in async operations and JSON processing
- Better HttpClient reuse patterns
- Optimized JSON parsing with better error handling
- Improved rate limit tracking efficiency
- Lower memory footprint due to System.Text.Json's efficient design

### Reliability Enhancements
- Automatic retry for transient network failures
- Better handling of timeout scenarios
- Enhanced error reporting with structured logging
- Improved connection management

---

## Previous Versions

For changes in previous versions, please refer to the git history or previous release notes.
