# Testing and Best Practices Guide

This comprehensive guide covers testing strategies, best practices, and common pitfalls to avoid when working with Unsplasharp.

## Table of Contents

- [Testing Strategies](#testing-strategies)
- [Unit Testing](#unit-testing)
- [Integration Testing](#integration-testing)
- [Performance Testing](#performance-testing)
- [Best Practices](#best-practices)
- [Common Pitfalls](#common-pitfalls)
- [Security Considerations](#security-considerations)
- [Production Deployment](#production-deployment)
- [Monitoring and Observability](#monitoring-and-observability)

## Testing Strategies

### Test Pyramid for Unsplasharp Applications

```
    /\
   /  \     E2E Tests (Few)
  /____\    - Full application flow
 /      \   - Real API integration
/__________\ Integration Tests (Some)
            - API contract testing
            - Error handling scenarios
            
Unit Tests (Many)
- Business logic
- Error handling
- Caching behavior
- Data transformation
```

### Testing Approach

1. **Unit Tests (70%)**: Test business logic, error handling, and data transformations
2. **Integration Tests (20%)**: Test API interactions and error scenarios
3. **End-to-End Tests (10%)**: Test complete user workflows

## Unit Testing

### Setting Up Test Infrastructure

```csharp
[TestFixture]
public class PhotoServiceTests
{
    private Mock<UnsplasharpClient> _mockClient;
    private Mock<IMemoryCache> _mockCache;
    private Mock<ILogger<PhotoService>> _mockLogger;
    private PhotoService _photoService;
    private TestData _testData;

    [SetUp]
    public void Setup()
    {
        _mockClient = new Mock<UnsplasharpClient>("test-app-id");
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<PhotoService>>();
        _testData = new TestData();
        
        _photoService = new PhotoService(_mockClient.Object, _mockCache.Object, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _photoService?.Dispose();
    }
}

public class TestData
{
    public Photo CreateValidPhoto(string id = null)
    {
        return new Photo
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Description = "Test photo description",
            Width = 1920,
            Height = 1080,
            Color = "#FF5733",
            Likes = 150,
            Downloads = 500,
            CreatedAt = DateTime.UtcNow.AddDays(-30).ToString("O"),
            UpdatedAt = DateTime.UtcNow.AddDays(-1).ToString("O"),
            User = CreateValidUser(),
            Urls = CreateValidUrls(),
            Exif = CreateValidExif(),
            Location = CreateValidLocation()
        };
    }

    public User CreateValidUser(string username = null)
    {
        return new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = username ?? "testuser",
            Name = "Test User",
            Bio = "Test photographer",
            TotalPhotos = 100,
            TotalLikes = 5000,
            ProfileImage = new ProfileImage
            {
                Small = "https://example.com/profile-small.jpg",
                Medium = "https://example.com/profile-medium.jpg",
                Large = "https://example.com/profile-large.jpg"
            }
        };
    }

    public Urls CreateValidUrls()
    {
        return new Urls
        {
            Thumbnail = "https://example.com/thumb.jpg",
            Small = "https://example.com/small.jpg",
            Regular = "https://example.com/regular.jpg",
            Full = "https://example.com/full.jpg",
            Raw = "https://example.com/raw.jpg"
        };
    }

    public Exif CreateValidExif()
    {
        return new Exif
        {
            Make = "Canon",
            Model = "EOS 5D Mark IV",
            Aperture = "2.8",
            ExposureTime = "1/125",
            Iso = 400,
            FocalLength = "85mm"
        };
    }

    public Location CreateValidLocation()
    {
        return new Location
        {
            Name = "Paris, France",
            City = "Paris",
            Country = "France",
            Position = new Position
            {
                Latitude = 48.8566,
                Longitude = 2.3522
            }
        };
    }
}
```

### Testing Success Scenarios

```csharp
[Test]
public async Task GetRandomPhoto_Success_ReturnsPhoto()
{
    // Arrange
    var expectedPhoto = _testData.CreateValidPhoto();
    _mockClient.Setup(c => c.GetRandomPhotoAsync(It.IsAny<CancellationToken>()))
              .ReturnsAsync(expectedPhoto);

    // Act
    var result = await _photoService.GetRandomPhotoAsync();

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(expectedPhoto.Id, result.Id);
    Assert.AreEqual(expectedPhoto.Description, result.Description);
    Assert.AreEqual(expectedPhoto.User.Name, result.User.Name);
    
    // Verify the mock was called correctly
    _mockClient.Verify(c => c.GetRandomPhotoAsync(It.IsAny<CancellationToken>()), Times.Once);
}

[Test]
public async Task SearchPhotos_WithValidQuery_ReturnsFilteredResults()
{
    // Arrange
    var query = "nature";
    var photos = new List<Photo>
    {
        _testData.CreateValidPhoto("1"),
        _testData.CreateValidPhoto("2"),
        _testData.CreateValidPhoto("3")
    };
    
    _mockClient.Setup(c => c.SearchPhotosAsync(
        query, 
        It.IsAny<int>(), 
        It.IsAny<int>(), 
        It.IsAny<OrderBy>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<Orientation>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(photos);

    // Act
    var result = await _photoService.SearchPhotosAsync(query);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(photos.Count, result.Count);
    CollectionAssert.AreEqual(photos.Select(p => p.Id), result.Select(p => p.Id));
}
```

### Testing Error Scenarios

```csharp
[Test]
public async Task GetPhoto_NotFound_ReturnsNull()
{
    // Arrange
    var photoId = "non-existent-id";
    _mockClient.Setup(c => c.GetPhotoAsync(photoId, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new UnsplasharpNotFoundException("Photo not found", photoId, "Photo", "https://api.unsplash.com/photos/non-existent-id", "GET", new ErrorContext("test-app")));

    // Act
    var result = await _photoService.GetPhotoSafelyAsync(photoId);

    // Assert
    Assert.IsNull(result);
    
    // Verify logging
    VerifyLogCalled(LogLevel.Warning, "Photo non-existent-id not found");
}

[Test]
public async Task GetPhoto_RateLimited_ThrowsException()
{
    // Arrange
    var photoId = "test-id";
    var rateLimitException = new UnsplasharpRateLimitException(
        "Rate limit exceeded", 
        0, 5000, 
        DateTimeOffset.UtcNow.AddMinutes(15),
        "https://api.unsplash.com/photos/test-id", 
        "GET", 
        new ErrorContext("test-app"));
    
    _mockClient.Setup(c => c.GetPhotoAsync(photoId, It.IsAny<CancellationToken>()))
              .ThrowsAsync(rateLimitException);

    // Act & Assert
    var ex = await Assert.ThrowsAsync<UnsplasharpRateLimitException>(
        () => _photoService.GetPhotoAsync(photoId));
    
    Assert.AreEqual(0, ex.RateLimitRemaining);
    Assert.AreEqual(5000, ex.RateLimit);
    Assert.IsNotNull(ex.RateLimitReset);
}

[Test]
public async Task SearchPhotos_NetworkError_RetriesAndFails()
{
    // Arrange
    var query = "test";
    var networkException = new UnsplasharpNetworkException(
        "Network error", 
        new HttpRequestException("Connection failed"),
        true, // IsRetryable
        "https://api.unsplash.com/search/photos",
        "GET",
        new ErrorContext("test-app"));
    
    _mockClient.Setup(c => c.SearchPhotosAsync(
        It.IsAny<string>(), 
        It.IsAny<int>(), 
        It.IsAny<int>(), 
        It.IsAny<OrderBy>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<Orientation>(),
        It.IsAny<CancellationToken>()))
        .ThrowsAsync(networkException);

    // Act & Assert
    var ex = await Assert.ThrowsAsync<UnsplasharpNetworkException>(
        () => _photoService.SearchPhotosAsync(query));
    
    Assert.IsTrue(ex.IsRetryable);
    
    // Verify retry attempts (if implemented)
    _mockClient.Verify(c => c.SearchPhotosAsync(
        It.IsAny<string>(), 
        It.IsAny<int>(), 
        It.IsAny<int>(), 
        It.IsAny<OrderBy>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<Orientation>(),
        It.IsAny<CancellationToken>()), 
        Times.AtLeastOnce);
}

private void VerifyLogCalled(LogLevel level, string message)
{
    _mockLogger.Verify(
        x => x.Log(
            level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}
```

### Testing Caching Behavior

```csharp
[Test]
public async Task GetPhoto_CacheHit_ReturnsFromCache()
{
    // Arrange
    var photoId = "cached-photo";
    var cachedPhoto = _testData.CreateValidPhoto(photoId);
    
    object cacheValue = cachedPhoto;
    _mockCache.Setup(c => c.TryGetValue($"photo:{photoId}", out cacheValue))
              .Returns(true);

    // Act
    var result = await _photoService.GetPhotoAsync(photoId);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(cachedPhoto.Id, result.Id);
    
    // Verify API was not called
    _mockClient.Verify(c => c.GetPhotoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    
    // Verify cache was checked
    _mockCache.Verify(c => c.TryGetValue($"photo:{photoId}", out It.Ref<object>.IsAny), Times.Once);
}

[Test]
public async Task GetPhoto_CacheMiss_FetchesAndCaches()
{
    // Arrange
    var photoId = "new-photo";
    var photo = _testData.CreateValidPhoto(photoId);
    
    object cacheValue = null;
    _mockCache.Setup(c => c.TryGetValue($"photo:{photoId}", out cacheValue))
              .Returns(false);
    
    _mockClient.Setup(c => c.GetPhotoAsync(photoId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(photo);

    // Act
    var result = await _photoService.GetPhotoAsync(photoId);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(photo.Id, result.Id);
    
    // Verify API was called
    _mockClient.Verify(c => c.GetPhotoAsync(photoId, It.IsAny<CancellationToken>()), Times.Once);
    
    // Verify caching occurred
    _mockCache.Verify(c => c.Set(
        $"photo:{photoId}", 
        photo, 
        It.IsAny<TimeSpan>()), Times.Once);
}
```

## Integration Testing

### Test Configuration

```csharp
[TestFixture]
[Category("Integration")]
public class UnsplashIntegrationTests
{
    private UnsplasharpClient _client;
    private ILogger<UnsplashIntegrationTests> _logger;
    private readonly string _testApplicationId;

    public UnsplashIntegrationTests()
    {
        // Use environment variable or test configuration
        _testApplicationId = Environment.GetEnvironmentVariable("UNSPLASH_TEST_APP_ID")
                           ?? throw new InvalidOperationException("UNSPLASH_TEST_APP_ID environment variable not set");
    }

    [SetUp]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        _logger = loggerFactory.CreateLogger<UnsplashIntegrationTests>();

        _client = new UnsplasharpClient(_testApplicationId, logger: _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }
}
```

### API Contract Testing

```csharp
[Test]
[Retry(3)] // Retry on rate limit or network issues
public async Task GetRandomPhoto_ReturnsValidPhotoStructure()
{
    // Act
    var photo = await _client.GetRandomPhotoAsync();

    // Assert - Validate complete photo structure
    Assert.IsNotNull(photo);

    // Basic properties
    Assert.IsNotEmpty(photo.Id);
    Assert.Greater(photo.Width, 0);
    Assert.Greater(photo.Height, 0);
    Assert.IsNotEmpty(photo.Color);
    Assert.GreaterOrEqual(photo.Likes, 0);
    Assert.GreaterOrEqual(photo.Downloads, 0);

    // User information
    Assert.IsNotNull(photo.User);
    Assert.IsNotEmpty(photo.User.Id);
    Assert.IsNotEmpty(photo.User.Name);
    Assert.IsNotEmpty(photo.User.Username);

    // URLs
    Assert.IsNotNull(photo.Urls);
    Assert.IsTrue(Uri.IsWellFormedUriString(photo.Urls.Thumbnail, UriKind.Absolute));
    Assert.IsTrue(Uri.IsWellFormedUriString(photo.Urls.Small, UriKind.Absolute));
    Assert.IsTrue(Uri.IsWellFormedUriString(photo.Urls.Regular, UriKind.Absolute));
    Assert.IsTrue(Uri.IsWellFormedUriString(photo.Urls.Full, UriKind.Absolute));
    Assert.IsTrue(Uri.IsWellFormedUriString(photo.Urls.Raw, UriKind.Absolute));

    // Timestamps
    Assert.DoesNotThrow(() => DateTime.Parse(photo.CreatedAt));
    Assert.DoesNotThrow(() => DateTime.Parse(photo.UpdatedAt));

    _logger.LogInformation("Successfully validated photo structure for {PhotoId}", photo.Id);
}

[Test]
public async Task SearchPhotos_WithValidQuery_ReturnsRelevantResults()
{
    // Arrange
    var query = "nature";
    var expectedMinResults = 5;

    // Act
    var photos = await _client.SearchPhotosAsync(query, perPage: 10);

    // Assert
    Assert.IsNotNull(photos);
    Assert.GreaterOrEqual(photos.Count, expectedMinResults);
    Assert.Greater(_client.LastPhotosSearchTotalResults, 0);
    Assert.Greater(_client.LastPhotosSearchTotalPages, 0);

    // Validate each photo
    foreach (var photo in photos)
    {
        Assert.IsNotEmpty(photo.Id);
        Assert.IsNotNull(photo.User);
        Assert.IsNotNull(photo.Urls);

        // Check if description or tags might contain the search term
        var searchRelevant = photo.Description?.ToLowerInvariant().Contains(query.ToLowerInvariant()) == true ||
                           photo.Categories.Any(c => c.Title.ToLowerInvariant().Contains(query.ToLowerInvariant()));

        // Note: Not all results may contain the exact term due to Unsplash's search algorithm
        _logger.LogDebug("Photo {PhotoId}: {Description} - Search relevant: {Relevant}",
            photo.Id, photo.Description, searchRelevant);
    }
}

[Test]
public async Task GetPhoto_WithInvalidId_ThrowsNotFoundException()
{
    // Arrange
    var invalidId = "definitely-not-a-real-photo-id-12345";

    // Act & Assert
    var ex = await Assert.ThrowsAsync<UnsplasharpNotFoundException>(
        () => _client.GetPhotoAsync(invalidId));

    Assert.AreEqual(invalidId, ex.ResourceId);
    Assert.AreEqual("Photo", ex.ResourceType);
    Assert.IsNotNull(ex.Context);
    Assert.IsNotEmpty(ex.Context.CorrelationId);

    _logger.LogInformation("Correctly handled not found exception for {PhotoId}", invalidId);
}
```

### Rate Limit Testing

```csharp
[Test]
[Explicit("Only run when testing rate limits")]
public async Task RateLimit_ExceedsLimit_HandlesGracefully()
{
    var successCount = 0;
    var rateLimitCount = 0;
    var tasks = new List<Task>();

    // Create many concurrent requests to trigger rate limiting
    for (int i = 0; i < 100; i++)
    {
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                await _client.GetRandomPhotoAsync();
                Interlocked.Increment(ref successCount);
            }
            catch (UnsplasharpRateLimitException ex)
            {
                Interlocked.Increment(ref rateLimitCount);
                _logger.LogWarning("Rate limited: {Remaining}/{Total}, Reset: {Reset}",
                    ex.RateLimitRemaining, ex.RateLimit, ex.RateLimitReset);
            }
        }));
    }

    await Task.WhenAll(tasks);

    _logger.LogInformation("Rate limit test completed: {Success} successful, {RateLimited} rate limited",
        successCount, rateLimitCount);

    Assert.Greater(successCount, 0, "At least some requests should succeed");
    Assert.Greater(rateLimitCount, 0, "Should hit rate limits with 100 concurrent requests");
}

[Test]
public async Task RateLimit_CheckHeaders_UpdatesClientState()
{
    // Act
    await _client.GetRandomPhotoAsync();

    // Assert
    Assert.Greater(_client.MaxRateLimit, 0);
    Assert.GreaterOrEqual(_client.RateLimitRemaining, 0);
    Assert.LessOrEqual(_client.RateLimitRemaining, _client.MaxRateLimit);

    _logger.LogInformation("Rate limit status: {Remaining}/{Max}",
        _client.RateLimitRemaining, _client.MaxRateLimit);
}
```

## Performance Testing

### Load Testing

```csharp
[TestFixture]
[Category("Performance")]
public class PerformanceTests
{
    private UnsplasharpClient _client;
    private ILogger<PerformanceTests> _logger;

    [SetUp]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        _logger = loggerFactory.CreateLogger<PerformanceTests>();

        _client = new UnsplasharpClient(Environment.GetEnvironmentVariable("UNSPLASH_TEST_APP_ID"));
    }

    [Test]
    [Explicit("Performance test - run manually")]
    public async Task Performance_ConcurrentRequests_MeasuresThroughput()
    {
        const int concurrentRequests = 10;
        const int requestsPerWorker = 5;

        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;
        var errorCount = 0;

        var tasks = Enumerable.Range(0, concurrentRequests).Select(async workerId =>
        {
            for (int i = 0; i < requestsPerWorker; i++)
            {
                try
                {
                    await _client.GetRandomPhotoAsync();
                    Interlocked.Increment(ref successCount);
                }
                catch (UnsplasharpRateLimitException)
                {
                    // Expected under load
                    Interlocked.Increment(ref errorCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in worker {WorkerId}, request {RequestId}", workerId, i);
                    Interlocked.Increment(ref errorCount);
                }

                // Small delay to avoid overwhelming the API
                await Task.Delay(100);
            }
        });

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        var totalRequests = successCount + errorCount;
        var throughput = totalRequests / stopwatch.Elapsed.TotalSeconds;

        _logger.LogInformation("Performance test completed:");
        _logger.LogInformation("  Total requests: {Total}", totalRequests);
        _logger.LogInformation("  Successful: {Success}", successCount);
        _logger.LogInformation("  Errors: {Errors}", errorCount);
        _logger.LogInformation("  Duration: {Duration:F2}s", stopwatch.Elapsed.TotalSeconds);
        _logger.LogInformation("  Throughput: {Throughput:F2} req/s", throughput);

        Assert.Greater(successCount, 0);
        Assert.Greater(throughput, 0.5); // At least 0.5 requests per second
    }

    [Test]
    public async Task Performance_ResponseTime_WithinAcceptableLimits()
    {
        const int iterations = 10;
        var responseTimes = new List<TimeSpan>();

        for (int i = 0; i < iterations; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _client.GetRandomPhotoAsync();
                stopwatch.Stop();
                responseTimes.Add(stopwatch.Elapsed);
            }
            catch (UnsplasharpRateLimitException)
            {
                // Skip rate limited requests
                continue;
            }

            // Small delay between requests
            await Task.Delay(200);
        }

        if (responseTimes.Count == 0)
        {
            Assert.Inconclusive("All requests were rate limited");
        }

        var averageResponseTime = TimeSpan.FromMilliseconds(responseTimes.Average(t => t.TotalMilliseconds));
        var maxResponseTime = responseTimes.Max();

        _logger.LogInformation("Response time analysis:");
        _logger.LogInformation("  Average: {Average:F0}ms", averageResponseTime.TotalMilliseconds);
        _logger.LogInformation("  Maximum: {Max:F0}ms", maxResponseTime.TotalMilliseconds);
        _logger.LogInformation("  Samples: {Count}", responseTimes.Count);

        Assert.Less(averageResponseTime.TotalSeconds, 5.0, "Average response time should be under 5 seconds");
        Assert.Less(maxResponseTime.TotalSeconds, 10.0, "Maximum response time should be under 10 seconds");
    }
}
```

### Memory Usage Testing

```csharp
[Test]
[Explicit("Memory test - run manually")]
public async Task Memory_MultipleRequests_NoMemoryLeaks()
{
    const int iterations = 100;

    // Force garbage collection before test
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var initialMemory = GC.GetTotalMemory(false);

    for (int i = 0; i < iterations; i++)
    {
        try
        {
            var photo = await _client.GetRandomPhotoAsync();

            // Process the photo to ensure it's not optimized away
            var info = $"{photo.Id}:{photo.Width}x{photo.Height}";

            if (i % 10 == 0)
            {
                var currentMemory = GC.GetTotalMemory(false);
                _logger.LogDebug("Iteration {Iteration}: Memory usage {Memory:N0} bytes", i, currentMemory);
            }
        }
        catch (UnsplasharpRateLimitException)
        {
            // Wait and continue
            await Task.Delay(1000);
        }
    }

    // Force garbage collection after test
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var finalMemory = GC.GetTotalMemory(false);
    var memoryIncrease = finalMemory - initialMemory;

    _logger.LogInformation("Memory usage analysis:");
    _logger.LogInformation("  Initial: {Initial:N0} bytes", initialMemory);
    _logger.LogInformation("  Final: {Final:N0} bytes", finalMemory);
    _logger.LogInformation("  Increase: {Increase:N0} bytes", memoryIncrease);

    // Allow for some memory increase, but not excessive
    var maxAllowedIncrease = 10 * 1024 * 1024; // 10MB
    Assert.Less(memoryIncrease, maxAllowedIncrease,
        $"Memory increase should be less than {maxAllowedIncrease:N0} bytes");
}
```

## Best Practices

### 1. Error Handling Best Practices

```csharp
// ✅ Good: Specific exception handling
public async Task<Photo?> GetPhotoSafely(string photoId)
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
    catch (UnsplasharpRateLimitException ex)
    {
        _logger.LogWarning("Rate limited for photo {PhotoId}. Reset: {Reset}", photoId, ex.RateLimitReset);
        throw; // Re-throw to let caller handle rate limiting
    }
    catch (UnsplasharpAuthenticationException ex)
    {
        _logger.LogError(ex, "Authentication failed - check API key");
        throw; // Critical error - should not continue
    }
    catch (UnsplasharpNetworkException ex) when (ex.IsRetryable)
    {
        _logger.LogWarning("Retryable network error for photo {PhotoId}: {Message}", photoId, ex.Message);
        throw; // Let retry logic handle this
    }
    catch (UnsplasharpException ex)
    {
        _logger.LogError(ex, "Unexpected Unsplash error for photo {PhotoId}", photoId);
        throw;
    }
}

// ❌ Bad: Generic exception handling
public async Task<Photo?> GetPhotoBadly(string photoId)
{
    try
    {
        return await _client.GetPhotoAsync(photoId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting photo");
        return null; // Loses important error information
    }
}
```

### 2. Caching Best Practices

```csharp
// ✅ Good: Intelligent caching with appropriate TTL
public class PhotoCacheService
{
    private readonly IMemoryCache _cache;
    private readonly UnsplasharpClient _client;

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

            // Cache popular photos longer
            var cacheDuration = photo.Likes > 1000
                ? TimeSpan.FromHours(2)
                : TimeSpan.FromMinutes(30);

            _cache.Set(cacheKey, photo, cacheDuration);
            return photo;
        }
        catch (UnsplasharpNotFoundException)
        {
            // Cache negative results for shorter time
            _cache.Set(cacheKey, (Photo?)null, TimeSpan.FromMinutes(5));
            return null;
        }
    }
}

// ❌ Bad: No caching or inappropriate caching
public class BadPhotoService
{
    public async Task<Photo?> GetPhotoAsync(string photoId)
    {
        // Always hits API - no caching
        return await _client.GetPhotoAsync(photoId);
    }

    // Or caching everything for the same duration
    public async Task<Photo?> GetPhotoCachedBadly(string photoId)
    {
        var cacheKey = $"photo:{photoId}";

        if (_cache.TryGetValue(cacheKey, out Photo cachedPhoto))
            return cachedPhoto;

        var photo = await _client.GetPhotoAsync(photoId);

        // Bad: Same cache duration for all photos
        _cache.Set(cacheKey, photo, TimeSpan.FromDays(1)); // Too long!
        return photo;
    }
}
```

### 3. Rate Limiting Best Practices

```csharp
// ✅ Good: Proactive rate limit monitoring
public class RateLimitAwareService
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<RateLimitAwareService> _logger;

    public async Task<List<Photo>> GetMultiplePhotosAsync(IEnumerable<string> photoIds)
    {
        var photos = new List<Photo>();

        foreach (var photoId in photoIds)
        {
            // Check rate limit before making request
            if (_client.RateLimitRemaining < 10)
            {
                _logger.LogWarning("Rate limit running low: {Remaining}/{Max}. Pausing requests.",
                    _client.RateLimitRemaining, _client.MaxRateLimit);

                await Task.Delay(TimeSpan.FromSeconds(30));
            }

            try
            {
                var photo = await _client.GetPhotoAsync(photoId);
                photos.Add(photo);
            }
            catch (UnsplasharpRateLimitException ex)
            {
                _logger.LogWarning("Rate limited. Waiting until {Reset}", ex.RateLimitReset);

                if (ex.TimeUntilReset.HasValue)
                {
                    await Task.Delay(ex.TimeUntilReset.Value);

                    // Retry the request
                    var photo = await _client.GetPhotoAsync(photoId);
                    photos.Add(photo);
                }
                break; // Stop processing if rate limited
            }
        }

        return photos;
    }
}

// ❌ Bad: Ignoring rate limits
public class BadRateLimitService
{
    public async Task<List<Photo>> GetMultiplePhotosBadly(IEnumerable<string> photoIds)
    {
        var tasks = photoIds.Select(id => _client.GetPhotoAsync(id));

        try
        {
            var photos = await Task.WhenAll(tasks);
            return photos.ToList();
        }
        catch (UnsplasharpRateLimitException)
        {
            // Bad: Just give up on rate limit
            return new List<Photo>();
        }
    }
}
```

### 4. Dependency Injection Best Practices

```csharp
// ✅ Good: Proper DI setup
public void ConfigureServices(IServiceCollection services)
{
    // Configure Unsplasharp with proper lifetime
    services.AddUnsplasharp(options =>
    {
        options.ApplicationId = Configuration["Unsplash:ApplicationId"];
        options.ConfigureHttpClient = client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        };
    });

    // Register services with appropriate lifetimes
    services.AddScoped<IPhotoService, PhotoService>();
    services.AddSingleton<IPhotoCache, PhotoCacheService>();
    services.AddMemoryCache();
}

// ❌ Bad: Manual instantiation in services
public class BadPhotoService
{
    private readonly UnsplasharpClient _client;

    public BadPhotoService()
    {
        // Bad: Creates new client instance, no DI
        _client = new UnsplasharpClient("hardcoded-app-id");
    }
}
```

## Common Pitfalls

### 1. Not Handling Rate Limits

```csharp
// ❌ Common mistake: Ignoring rate limits
public async Task<List<Photo>> SearchManyQueries(string[] queries)
{
    var allPhotos = new List<Photo>();

    foreach (var query in queries)
    {
        // This will likely hit rate limits
        var photos = await _client.SearchPhotosAsync(query, perPage: 30);
        allPhotos.AddRange(photos);
    }

    return allPhotos;
}

// ✅ Better approach: Rate limit aware
public async Task<List<Photo>> SearchManyQueriesSafely(string[] queries)
{
    var allPhotos = new List<Photo>();

    foreach (var query in queries)
    {
        try
        {
            var photos = await _client.SearchPhotosAsync(query, perPage: 30);
            allPhotos.AddRange(photos);

            // Courtesy delay between requests
            await Task.Delay(100);
        }
        catch (UnsplasharpRateLimitException ex)
        {
            _logger.LogWarning("Rate limited, waiting {Delay}ms", ex.TimeUntilReset?.TotalMilliseconds);

            if (ex.TimeUntilReset.HasValue)
            {
                await Task.Delay(ex.TimeUntilReset.Value);
            }

            // Optionally retry the failed query
        }
    }

    return allPhotos;
}
```

### 2. Improper HttpClient Usage

```csharp
// ❌ Bad: Creating HttpClient instances manually
public class BadDownloadService
{
    public async Task DownloadPhoto(string photoUrl, string filePath)
    {
        // Bad: Creates new HttpClient for each download
        using var httpClient = new HttpClient();
        var imageBytes = await httpClient.GetByteArrayAsync(photoUrl);
        await File.WriteAllBytesAsync(filePath, imageBytes);
    }
}

// ✅ Good: Using IHttpClientFactory
public class GoodDownloadService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GoodDownloadService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task DownloadPhoto(string photoUrl, string filePath)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var imageBytes = await httpClient.GetByteArrayAsync(photoUrl);
        await File.WriteAllBytesAsync(filePath, imageBytes);
    }
}
```

### 3. Not Using Cancellation Tokens

```csharp
// ❌ Bad: No cancellation support
public async Task<List<Photo>> SearchPhotosSlowly(string query)
{
    var photos = new List<Photo>();

    for (int page = 1; page <= 10; page++)
    {
        // No way to cancel this long-running operation
        var pagePhotos = await _client.SearchPhotosAsync(query, page: page);
        photos.AddRange(pagePhotos);
        await Task.Delay(1000); // Simulate slow processing
    }

    return photos;
}

// ✅ Good: Cancellation token support
public async Task<List<Photo>> SearchPhotosWithCancellation(string query, CancellationToken cancellationToken)
{
    var photos = new List<Photo>();

    for (int page = 1; page <= 10; page++)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pagePhotos = await _client.SearchPhotosAsync(query, page: page, cancellationToken: cancellationToken);
        photos.AddRange(pagePhotos);

        await Task.Delay(1000, cancellationToken);
    }

    return photos;
}
```

## Security Considerations

### 1. API Key Management

```csharp
// ✅ Good: Secure API key management
public void ConfigureServices(IServiceCollection services)
{
    // Use configuration system
    var unsplashConfig = Configuration.GetSection("Unsplash");
    var applicationId = unsplashConfig["ApplicationId"]
        ?? throw new InvalidOperationException("Unsplash ApplicationId not configured");

    services.AddUnsplasharp(applicationId);
}

// appsettings.json (for development)
{
  "Unsplash": {
    "ApplicationId": "your-dev-app-id"
  }
}

// Use Azure Key Vault, AWS Secrets Manager, or similar for production
// ❌ Bad: Hardcoded API keys
public class BadService
{
    private readonly UnsplasharpClient _client = new("hardcoded-api-key-123");
}
```

### 2. Input Validation

```csharp
// ✅ Good: Input validation
public async Task<List<Photo>> SearchPhotosSecurely(string query, int page = 1, int perPage = 20)
{
    // Validate inputs
    if (string.IsNullOrWhiteSpace(query))
        throw new ArgumentException("Query cannot be empty", nameof(query));

    if (query.Length > 100)
        throw new ArgumentException("Query too long", nameof(query));

    if (page < 1 || page > 1000)
        throw new ArgumentOutOfRangeException(nameof(page), "Page must be between 1 and 1000");

    if (perPage < 1 || perPage > 30)
        throw new ArgumentOutOfRangeException(nameof(perPage), "PerPage must be between 1 and 30");

    // Sanitize query to prevent injection attacks (if logging to external systems)
    var sanitizedQuery = query.Replace('\n', ' ').Replace('\r', ' ');

    return await _client.SearchPhotosAsync(sanitizedQuery, page: page, perPage: perPage);
}
```

### 3. Rate Limiting for User-Facing Applications

```csharp
// ✅ Good: Implement client-side rate limiting
public class UserRateLimitedPhotoService
{
    private readonly Dictionary<string, DateTime> _userLastRequest = new();
    private readonly TimeSpan _minRequestInterval = TimeSpan.FromSeconds(1);

    public async Task<List<Photo>> SearchPhotosForUser(string userId, string query)
    {
        // Implement per-user rate limiting
        if (_userLastRequest.TryGetValue(userId, out var lastRequest))
        {
            var timeSinceLastRequest = DateTime.UtcNow - lastRequest;
            if (timeSinceLastRequest < _minRequestInterval)
            {
                var waitTime = _minRequestInterval - timeSinceLastRequest;
                await Task.Delay(waitTime);
            }
        }

        _userLastRequest[userId] = DateTime.UtcNow;

        return await _client.SearchPhotosAsync(query);
    }
}
```

## Production Deployment

### 1. Configuration Management

```csharp
// Production-ready configuration
public class UnsplashConfiguration
{
    public string ApplicationId { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(1);
    public int MaxConcurrentRequests { get; set; } = 5;
}

// Startup configuration
public void ConfigureServices(IServiceCollection services)
{
    var unsplashConfig = Configuration.GetSection("Unsplash").Get<UnsplashConfiguration>();

    // Validate configuration
    if (string.IsNullOrEmpty(unsplashConfig?.ApplicationId))
    {
        throw new InvalidOperationException("Unsplash ApplicationId is required");
    }

    services.AddSingleton(unsplashConfig);

    services.AddUnsplasharp(options =>
    {
        options.ApplicationId = unsplashConfig.ApplicationId;
        options.Secret = unsplashConfig.Secret;
        options.ConfigureHttpClient = client =>
        {
            client.Timeout = unsplashConfig.DefaultTimeout;
        };
    });

    // Add health checks
    services.AddHealthChecks()
        .AddCheck<UnsplashHealthCheck>("unsplash");
}
```

### 2. Health Checks

```csharp
public class UnsplashHealthCheck : IHealthCheck
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<UnsplashHealthCheck> _logger;

    public UnsplashHealthCheck(UnsplasharpClient client, ILogger<UnsplashHealthCheck> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple health check - get total stats
            var stats = await _client.GetTotalStatsAsync(cancellationToken);

            if (stats == null)
            {
                return HealthCheckResult.Unhealthy("Unable to retrieve Unsplash statistics");
            }

            // Check rate limit status
            var rateLimitStatus = _client.RateLimitRemaining > 10
                ? "Healthy"
                : "Low";

            var data = new Dictionary<string, object>
            {
                ["rate_limit_remaining"] = _client.RateLimitRemaining,
                ["rate_limit_total"] = _client.MaxRateLimit,
                ["rate_limit_status"] = rateLimitStatus,
                ["total_photos"] = stats.Photos,
                ["last_check"] = DateTime.UtcNow
            };

            return _client.RateLimitRemaining > 0
                ? HealthCheckResult.Healthy("Unsplash API is accessible", data)
                : HealthCheckResult.Degraded("Rate limit exhausted", data);
        }
        catch (UnsplasharpRateLimitException ex)
        {
            _logger.LogWarning("Health check rate limited: {Message}", ex.Message);

            return HealthCheckResult.Degraded("Rate limited", new Dictionary<string, object>
            {
                ["rate_limit_reset"] = ex.RateLimitReset,
                ["error"] = ex.Message
            });
        }
        catch (UnsplasharpAuthenticationException ex)
        {
            _logger.LogError(ex, "Authentication failed during health check");

            return HealthCheckResult.Unhealthy("Authentication failed", new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["check_api_key"] = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");

            return HealthCheckResult.Unhealthy("Health check failed", new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["exception_type"] = ex.GetType().Name
            });
        }
    }
}
```

### 3. Graceful Degradation

```csharp
public class ResilientPhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ResilientPhotoService> _logger;
    private readonly CircuitBreakerPolicy _circuitBreaker;

    public ResilientPhotoService(
        UnsplasharpClient client,
        IMemoryCache cache,
        ILogger<ResilientPhotoService> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;

        // Configure circuit breaker
        _circuitBreaker = Policy
            .Handle<UnsplasharpException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (ex, duration) => _logger.LogWarning("Circuit breaker opened for {Duration}", duration),
                onReset: () => _logger.LogInformation("Circuit breaker reset"));
    }

    public async Task<Photo?> GetPhotoWithFallback(string photoId)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _client.GetPhotoAsync(photoId);
            });
        }
        catch (CircuitBreakerOpenException)
        {
            _logger.LogWarning("Circuit breaker open, using cached fallback for photo {PhotoId}", photoId);

            // Try to return cached version
            if (_cache.TryGetValue($"photo:{photoId}", out Photo cachedPhoto))
            {
                return cachedPhoto;
            }

            // Return placeholder or null
            return CreatePlaceholderPhoto(photoId);
        }
        catch (UnsplasharpRateLimitException ex)
        {
            _logger.LogWarning("Rate limited, using cached fallback for photo {PhotoId}", photoId);

            if (_cache.TryGetValue($"photo:{photoId}", out Photo cachedPhoto))
            {
                return cachedPhoto;
            }

            return null;
        }
    }

    private Photo CreatePlaceholderPhoto(string photoId)
    {
        return new Photo
        {
            Id = photoId,
            Description = "Photo temporarily unavailable",
            Width = 800,
            Height = 600,
            Color = "#CCCCCC",
            User = new User { Name = "Placeholder", Username = "placeholder" },
            Urls = new Urls
            {
                Regular = "https://via.placeholder.com/800x600?text=Photo+Unavailable",
                Small = "https://via.placeholder.com/400x300?text=Photo+Unavailable",
                Thumbnail = "https://via.placeholder.com/200x150?text=Photo+Unavailable"
            }
        };
    }
}
```

## Monitoring and Observability

### 1. Structured Logging

```csharp
public class ObservablePhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<ObservablePhotoService> _logger;
    private readonly IMetrics _metrics;

    public ObservablePhotoService(
        UnsplasharpClient client,
        ILogger<ObservablePhotoService> logger,
        IMetrics metrics)
    {
        _client = client;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<Photo?> GetPhotoAsync(string photoId)
    {
        using var activity = Activity.StartActivity("GetPhoto");
        activity?.SetTag("photo.id", photoId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Fetching photo {PhotoId}", photoId);

            var photo = await _client.GetPhotoAsync(photoId);

            stopwatch.Stop();

            _logger.LogInformation("Successfully fetched photo {PhotoId} in {Duration}ms",
                photoId, stopwatch.ElapsedMilliseconds);

            // Record metrics
            _metrics.Increment("unsplash.photo.requests", new[] { "status:success" });
            _metrics.Histogram("unsplash.photo.duration", stopwatch.ElapsedMilliseconds);

            activity?.SetTag("photo.width", photo.Width);
            activity?.SetTag("photo.height", photo.Height);
            activity?.SetTag("photo.likes", photo.Likes);

            return photo;
        }
        catch (UnsplasharpNotFoundException ex)
        {
            stopwatch.Stop();

            _logger.LogWarning("Photo {PhotoId} not found", photoId);

            _metrics.Increment("unsplash.photo.requests", new[] { "status:not_found" });

            activity?.SetStatus(ActivityStatusCode.Error, "Photo not found");

            return null;
        }
        catch (UnsplasharpRateLimitException ex)
        {
            stopwatch.Stop();

            _logger.LogWarning("Rate limited fetching photo {PhotoId}. Remaining: {Remaining}/{Total}, Reset: {Reset}",
                photoId, ex.RateLimitRemaining, ex.RateLimit, ex.RateLimitReset);

            _metrics.Increment("unsplash.photo.requests", new[] { "status:rate_limited" });
            _metrics.Gauge("unsplash.rate_limit.remaining", ex.RateLimitRemaining ?? 0);

            activity?.SetStatus(ActivityStatusCode.Error, "Rate limited");

            throw;
        }
        catch (UnsplasharpException ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Error fetching photo {PhotoId}: {ErrorMessage}", photoId, ex.Message);

            _metrics.Increment("unsplash.photo.requests", new[] { "status:error" });

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }
}
```

### 2. Custom Metrics

```csharp
public class UnsplashMetricsService
{
    private readonly IMetrics _metrics;
    private readonly Timer _metricsTimer;

    public UnsplashMetricsService(IMetrics metrics, UnsplasharpClient client)
    {
        _metrics = metrics;

        // Periodically report rate limit status
        _metricsTimer = new Timer(async _ =>
        {
            _metrics.Gauge("unsplash.rate_limit.remaining", client.RateLimitRemaining);
            _metrics.Gauge("unsplash.rate_limit.total", client.MaxRateLimit);

            var utilizationPercent = client.MaxRateLimit > 0
                ? (double)(client.MaxRateLimit - client.RateLimitRemaining) / client.MaxRateLimit * 100
                : 0;

            _metrics.Gauge("unsplash.rate_limit.utilization_percent", utilizationPercent);

        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    public void RecordSearchMetrics(string query, int resultCount, TimeSpan duration)
    {
        _metrics.Increment("unsplash.search.requests");
        _metrics.Histogram("unsplash.search.duration", duration.TotalMilliseconds);
        _metrics.Histogram("unsplash.search.result_count", resultCount);

        // Tag by query type
        var queryType = DetermineQueryType(query);
        _metrics.Increment("unsplash.search.by_type", new[] { $"type:{queryType}" });
    }

    private string DetermineQueryType(string query)
    {
        if (query.Contains(" ")) return "multi_word";
        if (query.Length > 20) return "long";
        if (query.All(char.IsLetter)) return "text_only";
        return "simple";
    }

    public void Dispose()
    {
        _metricsTimer?.Dispose();
    }
}
```

### 3. Application Insights Integration

```csharp
public class ApplicationInsightsPhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightsPhotoService(UnsplasharpClient client, TelemetryClient telemetryClient)
    {
        _client = client;
        _telemetryClient = telemetryClient;
    }

    public async Task<List<Photo>> SearchPhotosWithTelemetry(string query, int perPage = 20)
    {
        var stopwatch = Stopwatch.StartNew();
        var properties = new Dictionary<string, string>
        {
            ["query"] = query,
            ["per_page"] = perPage.ToString()
        };

        try
        {
            var photos = await _client.SearchPhotosAsync(query, perPage: perPage);

            stopwatch.Stop();

            var metrics = new Dictionary<string, double>
            {
                ["duration_ms"] = stopwatch.ElapsedMilliseconds,
                ["result_count"] = photos.Count,
                ["rate_limit_remaining"] = _client.RateLimitRemaining,
                ["total_results"] = _client.LastPhotosSearchTotalResults
            };

            _telemetryClient.TrackEvent("UnsplashSearchSuccess", properties, metrics);

            return photos;
        }
        catch (UnsplasharpRateLimitException ex)
        {
            stopwatch.Stop();

            properties["error_type"] = "rate_limit";
            properties["rate_limit_reset"] = ex.RateLimitReset?.ToString() ?? "unknown";

            var metrics = new Dictionary<string, double>
            {
                ["duration_ms"] = stopwatch.ElapsedMilliseconds,
                ["rate_limit_remaining"] = ex.RateLimitRemaining ?? 0,
                ["rate_limit_total"] = ex.RateLimit ?? 0
            };

            _telemetryClient.TrackEvent("UnsplashSearchRateLimited", properties, metrics);

            throw;
        }
        catch (UnsplasharpException ex)
        {
            stopwatch.Stop();

            properties["error_type"] = ex.GetType().Name;
            properties["error_message"] = ex.Message;

            var metrics = new Dictionary<string, double>
            {
                ["duration_ms"] = stopwatch.ElapsedMilliseconds
            };

            _telemetryClient.TrackEvent("UnsplashSearchError", properties, metrics);
            _telemetryClient.TrackException(ex);

            throw;
        }
    }
}
```

---

## Summary

This comprehensive testing and best practices guide provides:

### Testing Strategy
- **Unit Tests**: Focus on business logic and error handling
- **Integration Tests**: Validate API contracts and error scenarios
- **Performance Tests**: Ensure acceptable response times and throughput

### Best Practices
- **Error Handling**: Use specific exception types and appropriate logging
- **Caching**: Implement intelligent caching with appropriate TTL
- **Rate Limiting**: Proactive monitoring and graceful handling
- **Security**: Secure API key management and input validation

### Production Readiness
- **Configuration**: Environment-specific settings and validation
- **Health Checks**: Monitor API availability and rate limits
- **Resilience**: Circuit breakers and graceful degradation
- **Observability**: Structured logging, metrics, and distributed tracing

### Key Takeaways
1. Always test error scenarios, not just happy paths
2. Implement comprehensive monitoring and alerting
3. Use dependency injection for better testability
4. Handle rate limits proactively, not reactively
5. Cache intelligently based on content characteristics
6. Validate inputs and secure API keys properly
7. Plan for graceful degradation when the API is unavailable

Following these practices will help you build robust, maintainable, and production-ready applications with Unsplasharp.
```
```
```
