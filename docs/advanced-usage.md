# Advanced Usage Patterns

This guide covers advanced usage patterns, performance optimization techniques, and sophisticated integration strategies for Unsplasharp.

## Table of Contents

- [Advanced Pagination Strategies](#advanced-pagination-strategies)
- [Filtering and Search Optimization](#filtering-and-search-optimization)
- [Custom Parameters and URL Manipulation](#custom-parameters-and-url-manipulation)
- [Performance Optimization](#performance-optimization)
- [Batch Operations](#batch-operations)
- [Advanced Error Handling Patterns](#advanced-error-handling-patterns)
- [Custom HTTP Client Configuration](#custom-http-client-configuration)
- [Monitoring and Metrics](#monitoring-and-metrics)

## Advanced Pagination Strategies

### Infinite Scroll Implementation

```csharp
public class InfiniteScrollPhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<InfiniteScrollPhotoService> _logger;

    public InfiniteScrollPhotoService(UnsplasharpClient client, ILogger<InfiniteScrollPhotoService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async IAsyncEnumerable<Photo> GetPhotosAsync(
        string query, 
        int batchSize = 20,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int currentPage = 1;
        bool hasMoreResults = true;

        while (hasMoreResults && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var photos = await _client.SearchPhotosAsync(
                    query, 
                    page: currentPage, 
                    perPage: batchSize,
                    cancellationToken: cancellationToken);

                if (photos.Count == 0)
                {
                    hasMoreResults = false;
                    yield break;
                }

                foreach (var photo in photos)
                {
                    yield return photo;
                }

                // Check if we've reached the end
                hasMoreResults = photos.Count == batchSize && 
                                currentPage < _client.LastPhotosSearchTotalPages;
                
                currentPage++;

                // Rate limiting courtesy delay
                await Task.Delay(100, cancellationToken);
            }
            catch (UnsplasharpRateLimitException ex)
            {
                _logger.LogWarning("Rate limit hit during pagination, waiting {Delay}ms", 
                    ex.TimeUntilReset?.TotalMilliseconds ?? 60000);
                
                await Task.Delay(ex.TimeUntilReset ?? TimeSpan.FromMinutes(1), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during pagination at page {Page}", currentPage);
                hasMoreResults = false;
            }
        }
    }
}

// Usage example
public async Task UseInfiniteScroll()
{
    var service = new InfiniteScrollPhotoService(_client, _logger);
    var photoCount = 0;

    await foreach (var photo in service.GetPhotosAsync("nature", batchSize: 30))
    {
        Console.WriteLine($"{++photoCount}: {photo.Description} by {photo.User.Name}");
        
        // Process photo (e.g., add to UI, cache, etc.)
        
        if (photoCount >= 100) // Limit for demo
            break;
    }
}
```

### Parallel Pagination

```csharp
public class ParallelPaginationService
{
    private readonly UnsplasharpClient _client;
    private readonly SemaphoreSlim _semaphore;

    public ParallelPaginationService(UnsplasharpClient client, int maxConcurrency = 3)
    {
        _client = client;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    public async Task<List<Photo>> GetPhotosParallelAsync(
        string query, 
        int totalPages = 5, 
        int perPage = 30)
    {
        var tasks = new List<Task<List<Photo>>>();

        for (int page = 1; page <= totalPages; page++)
        {
            tasks.Add(GetPageAsync(query, page, perPage));
        }

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(photos => photos).ToList();
    }

    private async Task<List<Photo>> GetPageAsync(string query, int page, int perPage)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            // Add jitter to avoid thundering herd
            await Task.Delay(Random.Shared.Next(0, 500));
            
            return await _client.SearchPhotosAsync(query, page: page, perPage: perPage);
        }
        catch (UnsplasharpRateLimitException)
        {
            // If rate limited, return empty list and let caller handle
            return new List<Photo>();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

## Filtering and Search Optimization

### Advanced Search Builder

```csharp
public class UnsplashSearchBuilder
{
    private string _query = string.Empty;
    private int _page = 1;
    private int _perPage = 10;
    private OrderBy _orderBy = OrderBy.Relevant;
    private string? _color;
    private Orientation _orientation = Orientation.All;
    private string? _contentFilter;
    private List<string> _collectionIds = new();

    public UnsplashSearchBuilder Query(string query)
    {
        _query = query;
        return this;
    }

    public UnsplashSearchBuilder Page(int page)
    {
        _page = Math.Max(1, page);
        return this;
    }

    public UnsplashSearchBuilder PerPage(int perPage)
    {
        _perPage = Math.Clamp(perPage, 1, 30);
        return this;
    }

    public UnsplashSearchBuilder OrderBy(OrderBy orderBy)
    {
        _orderBy = orderBy;
        return this;
    }

    public UnsplashSearchBuilder Color(string color)
    {
        var validColors = new[] { "black_and_white", "black", "white", "yellow", 
                                 "orange", "red", "purple", "magenta", "green", "teal", "blue" };
        
        if (validColors.Contains(color.ToLowerInvariant()))
        {
            _color = color;
        }
        return this;
    }

    public UnsplashSearchBuilder Orientation(Orientation orientation)
    {
        _orientation = orientation;
        return this;
    }

    public UnsplashSearchBuilder ContentFilter(string filter)
    {
        if (filter == "low" || filter == "high")
        {
            _contentFilter = filter;
        }
        return this;
    }

    public UnsplashSearchBuilder InCollections(params string[] collectionIds)
    {
        _collectionIds.AddRange(collectionIds);
        return this;
    }

    public async Task<List<Photo>> ExecuteAsync(UnsplasharpClient client)
    {
        var collectionIdsString = _collectionIds.Count > 0 ? string.Join(",", _collectionIds) : null;
        
        return await client.SearchPhotosAsync(
            _query,
            _page,
            _perPage,
            _orderBy,
            collectionIdsString,
            _contentFilter,
            _color,
            _orientation
        );
    }
}

// Usage example
public async Task AdvancedSearchExample()
{
    var photos = await new UnsplashSearchBuilder()
        .Query("mountain landscape")
        .Color("blue")
        .Orientation(Orientation.Landscape)
        .OrderBy(OrderBy.Popular)
        .PerPage(20)
        .ContentFilter("high")
        .InCollections("499830", "194162")
        .ExecuteAsync(_client);

    Console.WriteLine($"Found {photos.Count} photos matching criteria");
}
```

### Smart Search with Fallbacks

```csharp
public class SmartSearchService
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<SmartSearchService> _logger;

    public SmartSearchService(UnsplasharpClient client, ILogger<SmartSearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<Photo>> SmartSearchAsync(string query, int desiredCount = 20)
    {
        var searchStrategies = new List<Func<Task<List<Photo>>>>
        {
            // Primary search - exact query
            () => _client.SearchPhotosAsync(query, perPage: desiredCount),
            
            // Fallback 1 - broader search with popular ordering
            () => _client.SearchPhotosAsync(query, orderBy: OrderBy.Popular, perPage: desiredCount),
            
            // Fallback 2 - search individual words
            () => SearchIndividualWords(query, desiredCount),
            
            // Fallback 3 - random photos if all else fails
            () => GetRandomPhotosAsync(desiredCount)
        };

        foreach (var strategy in searchStrategies)
        {
            try
            {
                var results = await strategy();
                if (results.Count > 0)
                {
                    _logger.LogInformation("Search strategy succeeded, found {Count} photos", results.Count);
                    return results;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Search strategy failed, trying next approach");
            }
        }

        _logger.LogWarning("All search strategies failed for query: {Query}", query);
        return new List<Photo>();
    }

    private async Task<List<Photo>> SearchIndividualWords(string query, int desiredCount)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var allPhotos = new List<Photo>();

        foreach (var word in words.Take(3)) // Limit to first 3 words
        {
            try
            {
                var photos = await _client.SearchPhotosAsync(word, perPage: desiredCount / words.Length + 5);
                allPhotos.AddRange(photos);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to search for word: {Word}", word);
            }
        }

        return allPhotos.DistinctBy(p => p.Id).Take(desiredCount).ToList();
    }

    private async Task<List<Photo>> GetRandomPhotosAsync(int count)
    {
        var photos = new List<Photo>();
        var batchSize = Math.Min(count, 30);
        
        for (int i = 0; i < Math.Ceiling((double)count / batchSize); i++)
        {
            try
            {
                var randomPhotos = await _client.GetRandomPhotosAsync(batchSize);
                photos.AddRange(randomPhotos);
                
                if (photos.Count >= count)
                    break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to get random photos batch {Batch}", i);
            }
        }

        return photos.Take(count).ToList();
    }
}
```

## Custom Parameters and URL Manipulation

### Custom Photo Sizing

```csharp
public class CustomPhotoService
{
    private readonly UnsplasharpClient _client;

    public CustomPhotoService(UnsplasharpClient client)
    {
        _client = client;
    }

    public async Task<Photo?> GetPhotoWithCustomSize(string photoId, int width, int height, bool crop = false)
    {
        if (crop)
        {
            // Get original photo first to calculate crop parameters
            var originalPhoto = await _client.GetPhotoAsync(photoId);

            // Calculate center crop
            var cropX = Math.Max(0, (originalPhoto.Width - width) / 2);
            var cropY = Math.Max(0, (originalPhoto.Height - height) / 2);

            return await _client.GetPhoto(photoId, width, height, cropX, cropY, width, height);
        }
        else
        {
            return await _client.GetPhoto(photoId, width, height);
        }
    }

    public async Task<Dictionary<string, string>> GetMultipleSizes(string photoId)
    {
        var sizes = new Dictionary<string, (int width, int height)>
        {
            ["thumbnail"] = (200, 200),
            ["small"] = (400, 300),
            ["medium"] = (800, 600),
            ["large"] = (1200, 900),
            ["hero"] = (1920, 1080)
        };

        var results = new Dictionary<string, string>();

        foreach (var (sizeName, (width, height)) in sizes)
        {
            try
            {
                var photo = await _client.GetPhoto(photoId, width, height);
                if (photo?.Urls.Custom != null)
                {
                    results[sizeName] = photo.Urls.Custom;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get {sizeName} size: {ex.Message}");
            }
        }

        return results;
    }
}
```

### URL Parameter Optimization

```csharp
public static class UnsplashUrlHelper
{
    public static string OptimizePhotoUrl(string originalUrl, int? width = null, int? height = null,
        int? quality = null, string? format = null, bool? fit = null)
    {
        if (string.IsNullOrEmpty(originalUrl))
            return originalUrl;

        var uriBuilder = new UriBuilder(originalUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        if (width.HasValue)
            query["w"] = width.Value.ToString();

        if (height.HasValue)
            query["h"] = height.Value.ToString();

        if (quality.HasValue && quality.Value >= 1 && quality.Value <= 100)
            query["q"] = quality.Value.ToString();

        if (!string.IsNullOrEmpty(format) && new[] { "jpg", "png", "webp" }.Contains(format.ToLower()))
            query["fm"] = format.ToLower();

        if (fit.HasValue)
            query["fit"] = fit.Value ? "crop" : "max";

        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    public static string AddWatermark(string photoUrl, string text, string position = "bottom-right")
    {
        var uriBuilder = new UriBuilder(photoUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query["txt"] = text;
        query["txt-pos"] = position;
        query["txt-size"] = "24";
        query["txt-color"] = "ffffff";

        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }
}

// Usage example
public async Task CustomUrlExample()
{
    var photo = await _client.GetPhotoAsync("qcs09SwNPHY");

    // Optimize for web display
    var webOptimized = UnsplashUrlHelper.OptimizePhotoUrl(
        photo.Urls.Regular,
        width: 800,
        height: 600,
        quality: 80,
        format: "webp"
    );

    // Add watermark
    var watermarked = UnsplashUrlHelper.AddWatermark(
        photo.Urls.Regular,
        "© My App",
        "bottom-right"
    );

    Console.WriteLine($"Original: {photo.Urls.Regular}");
    Console.WriteLine($"Optimized: {webOptimized}");
    Console.WriteLine($"Watermarked: {watermarked}");
}
```

## Performance Optimization

### Intelligent Caching Strategy

```csharp
public class IntelligentCacheService
{
    private readonly UnsplasharpClient _client;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<IntelligentCacheService> _logger;

    public IntelligentCacheService(
        UnsplasharpClient client,
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<IntelligentCacheService> logger)
    {
        _client = client;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<Photo?> GetPhotoAsync(string photoId, CacheStrategy strategy = CacheStrategy.Intelligent)
    {
        var cacheKey = $"photo:{photoId}";

        // Try memory cache first (fastest)
        if (_memoryCache.TryGetValue(cacheKey, out Photo cachedPhoto))
        {
            _logger.LogDebug("Photo {PhotoId} found in memory cache", photoId);
            return cachedPhoto;
        }

        // Try distributed cache (Redis, etc.)
        if (strategy >= CacheStrategy.Distributed)
        {
            var distributedData = await _distributedCache.GetStringAsync(cacheKey);
            if (distributedData != null)
            {
                try
                {
                    var photo = JsonSerializer.Deserialize<Photo>(distributedData);

                    // Store in memory cache for faster future access
                    _memoryCache.Set(cacheKey, photo, TimeSpan.FromMinutes(15));

                    _logger.LogDebug("Photo {PhotoId} found in distributed cache", photoId);
                    return photo;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached photo {PhotoId}", photoId);
                }
            }
        }

        // Fetch from API
        try
        {
            var photo = await _client.GetPhotoAsync(photoId);

            // Cache with intelligent TTL based on photo popularity
            var memoryCacheDuration = CalculateMemoryCacheDuration(photo);
            var distributedCacheDuration = CalculateDistributedCacheDuration(photo);

            _memoryCache.Set(cacheKey, photo, memoryCacheDuration);

            if (strategy >= CacheStrategy.Distributed)
            {
                var serializedPhoto = JsonSerializer.Serialize(photo);
                await _distributedCache.SetStringAsync(cacheKey, serializedPhoto,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = distributedCacheDuration
                    });
            }

            _logger.LogDebug("Photo {PhotoId} fetched from API and cached", photoId);
            return photo;
        }
        catch (UnsplasharpNotFoundException)
        {
            // Cache negative results to avoid repeated API calls
            _memoryCache.Set(cacheKey, (Photo?)null, TimeSpan.FromMinutes(5));
            return null;
        }
    }

    private TimeSpan CalculateMemoryCacheDuration(Photo photo)
    {
        // Popular photos (high likes/downloads) cached longer
        var popularity = photo.Likes + (photo.Downloads / 10);

        return popularity switch
        {
            > 10000 => TimeSpan.FromHours(2),
            > 1000 => TimeSpan.FromHours(1),
            > 100 => TimeSpan.FromMinutes(30),
            _ => TimeSpan.FromMinutes(15)
        };
    }

    private TimeSpan CalculateDistributedCacheDuration(Photo photo)
    {
        // Longer cache for distributed storage
        return CalculateMemoryCacheDuration(photo).Multiply(4);
    }
}

public enum CacheStrategy
{
    None,
    Memory,
    Distributed,
    Intelligent
}
```

### Connection Pooling and HTTP Optimization

```csharp
public static class UnsplashHttpClientConfiguration
{
    public static void ConfigureOptimizedHttpClient(this IServiceCollection services,
        UnsplashConfiguration config)
    {
        services.AddHttpClient("unsplash", client =>
        {
            client.BaseAddress = new Uri("https://api.unsplash.com/");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Client-ID", config.ApplicationId);

            // Optimize headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                $"UnsplasharpApp/1.0 (+{config.ApplicationUrl})");

            // Connection optimization
            client.Timeout = config.DefaultTimeout;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // Connection pooling settings
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 10,

            // Performance settings
            EnableMultipleHttp2Connections = true,
            UseCookies = false, // Unsplash API doesn't use cookies

            // Compression
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != HttpStatusCode.NotFound)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                                                      TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogWarning("Retry {RetryCount} after {Delay}ms for {Url}",
                        retryCount, timespan.TotalMilliseconds, context.OperationKey);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, timespan) =>
                {
                    // Log circuit breaker opening
                },
                onReset: () =>
                {
                    // Log circuit breaker closing
                });
    }
}
```

## Batch Operations

### Efficient Bulk Photo Processing

```csharp
public class BulkPhotoProcessor
{
    private readonly UnsplasharpClient _client;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<BulkPhotoProcessor> _logger;

    public BulkPhotoProcessor(UnsplasharpClient client, ILogger<BulkPhotoProcessor> logger, int maxConcurrency = 5)
    {
        _client = client;
        _logger = logger;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    public async Task<BulkProcessResult<Photo>> ProcessPhotosAsync(
        IEnumerable<string> photoIds,
        CancellationToken cancellationToken = default)
    {
        var result = new BulkProcessResult<Photo>();
        var tasks = photoIds.Select(id => ProcessSinglePhotoAsync(id, result, cancellationToken));

        await Task.WhenAll(tasks);

        _logger.LogInformation("Bulk processing completed: {Success} successful, {Failed} failed",
            result.Successful.Count, result.Failed.Count);

        return result;
    }

    private async Task ProcessSinglePhotoAsync(
        string photoId,
        BulkProcessResult<Photo> result,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            // Add jitter to prevent thundering herd
            await Task.Delay(Random.Shared.Next(0, 200), cancellationToken);

            var photo = await _client.GetPhotoAsync(photoId, cancellationToken);

            lock (result)
            {
                result.Successful.Add(photo);
            }

            _logger.LogDebug("Successfully processed photo {PhotoId}", photoId);
        }
        catch (UnsplasharpNotFoundException)
        {
            lock (result)
            {
                result.Failed.Add(new BulkProcessError(photoId, "Photo not found"));
            }
            _logger.LogWarning("Photo {PhotoId} not found", photoId);
        }
        catch (UnsplasharpRateLimitException ex)
        {
            lock (result)
            {
                result.Failed.Add(new BulkProcessError(photoId, "Rate limit exceeded"));
            }
            _logger.LogWarning("Rate limit exceeded for photo {PhotoId}", photoId);

            // Wait for rate limit reset
            if (ex.TimeUntilReset.HasValue)
            {
                await Task.Delay(ex.TimeUntilReset.Value, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            lock (result)
            {
                result.Failed.Add(new BulkProcessError(photoId, ex.Message));
            }
            _logger.LogError(ex, "Error processing photo {PhotoId}", photoId);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

public class BulkProcessResult<T>
{
    public List<T> Successful { get; } = new();
    public List<BulkProcessError> Failed { get; } = new();

    public int TotalProcessed => Successful.Count + Failed.Count;
    public double SuccessRate => TotalProcessed > 0 ? (double)Successful.Count / TotalProcessed : 0;
}

public record BulkProcessError(string Id, string Error);
```

### Batch Download Manager

```csharp
public class BatchDownloadManager
{
    private readonly UnsplasharpClient _client;
    private readonly HttpClient _httpClient;
    private readonly ILogger<BatchDownloadManager> _logger;
    private readonly SemaphoreSlim _downloadSemaphore;

    public BatchDownloadManager(
        UnsplasharpClient client,
        HttpClient httpClient,
        ILogger<BatchDownloadManager> logger,
        int maxConcurrentDownloads = 3)
    {
        _client = client;
        _httpClient = httpClient;
        _logger = logger;
        _downloadSemaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
    }

    public async Task<BatchDownloadResult> DownloadPhotosAsync(
        IEnumerable<string> photoIds,
        string downloadDirectory,
        PhotoSize size = PhotoSize.Regular,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(downloadDirectory);

        var result = new BatchDownloadResult();
        var photoIdsList = photoIds.ToList();
        var totalPhotos = photoIdsList.Count;
        var processedCount = 0;

        var downloadTasks = photoIdsList.Select(async photoId =>
        {
            try
            {
                var downloadPath = await DownloadSinglePhotoAsync(photoId, downloadDirectory, size, cancellationToken);

                lock (result)
                {
                    result.SuccessfulDownloads.Add(new DownloadResult(photoId, downloadPath));
                    processedCount++;
                }

                progress?.Report(new DownloadProgress(processedCount, totalPhotos, photoId, true));
            }
            catch (Exception ex)
            {
                lock (result)
                {
                    result.FailedDownloads.Add(new DownloadError(photoId, ex.Message));
                    processedCount++;
                }

                progress?.Report(new DownloadProgress(processedCount, totalPhotos, photoId, false));
                _logger.LogError(ex, "Failed to download photo {PhotoId}", photoId);
            }
        });

        await Task.WhenAll(downloadTasks);

        _logger.LogInformation("Batch download completed: {Success}/{Total} successful",
            result.SuccessfulDownloads.Count, totalPhotos);

        return result;
    }

    private async Task<string> DownloadSinglePhotoAsync(
        string photoId,
        string downloadDirectory,
        PhotoSize size,
        CancellationToken cancellationToken)
    {
        await _downloadSemaphore.WaitAsync(cancellationToken);

        try
        {
            // Get photo metadata
            var photo = await _client.GetPhotoAsync(photoId, cancellationToken);

            // Select appropriate URL based on size
            var imageUrl = size switch
            {
                PhotoSize.Thumbnail => photo.Urls.Thumbnail,
                PhotoSize.Small => photo.Urls.Small,
                PhotoSize.Regular => photo.Urls.Regular,
                PhotoSize.Full => photo.Urls.Full,
                PhotoSize.Raw => photo.Urls.Raw,
                _ => photo.Urls.Regular
            };

            // Download image
            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl, cancellationToken);

            // Generate filename
            var fileName = $"{photo.Id}_{size.ToString().ToLower()}.jpg";
            var filePath = Path.Combine(downloadDirectory, fileName);

            // Save to disk
            await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

            _logger.LogDebug("Downloaded photo {PhotoId} to {FilePath} ({Size} bytes)",
                photoId, filePath, imageBytes.Length);

            return filePath;
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }
}

public enum PhotoSize
{
    Thumbnail,
    Small,
    Regular,
    Full,
    Raw
}

public class BatchDownloadResult
{
    public List<DownloadResult> SuccessfulDownloads { get; } = new();
    public List<DownloadError> FailedDownloads { get; } = new();

    public int TotalAttempted => SuccessfulDownloads.Count + FailedDownloads.Count;
    public double SuccessRate => TotalAttempted > 0 ? (double)SuccessfulDownloads.Count / TotalAttempted : 0;
}

public record DownloadResult(string PhotoId, string FilePath);
public record DownloadError(string PhotoId, string Error);
public record DownloadProgress(int Processed, int Total, string CurrentPhotoId, bool Success);
```

## Monitoring and Metrics

### Performance Metrics Collection

```csharp
public class UnsplashMetricsCollector
{
    private readonly ILogger<UnsplashMetricsCollector> _logger;
    private readonly ConcurrentDictionary<string, ApiMetrics> _metrics = new();

    public UnsplashMetricsCollector(ILogger<UnsplashMetricsCollector> logger)
    {
        _logger = logger;
    }

    public void RecordApiCall(string endpoint, TimeSpan duration, bool success, int? statusCode = null)
    {
        var metrics = _metrics.GetOrAdd(endpoint, _ => new ApiMetrics());

        lock (metrics)
        {
            metrics.TotalCalls++;
            metrics.TotalDuration += duration;

            if (success)
            {
                metrics.SuccessfulCalls++;
            }
            else
            {
                metrics.FailedCalls++;
            }

            if (statusCode.HasValue)
            {
                metrics.StatusCodes.AddOrUpdate(statusCode.Value, 1, (_, count) => count + 1);
            }

            metrics.LastCallTime = DateTimeOffset.UtcNow;

            // Update min/max duration
            if (duration < metrics.MinDuration || metrics.MinDuration == TimeSpan.Zero)
                metrics.MinDuration = duration;

            if (duration > metrics.MaxDuration)
                metrics.MaxDuration = duration;
        }
    }

    public void RecordRateLimit(int remaining, int limit, DateTimeOffset? resetTime)
    {
        var rateLimitMetrics = _metrics.GetOrAdd("_rate_limit", _ => new ApiMetrics());

        lock (rateLimitMetrics)
        {
            rateLimitMetrics.RateLimitRemaining = remaining;
            rateLimitMetrics.RateLimitTotal = limit;
            rateLimitMetrics.RateLimitResetTime = resetTime;
        }
    }

    public MetricsSummary GetSummary()
    {
        var summary = new MetricsSummary
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            EndpointMetrics = new Dictionary<string, EndpointSummary>()
        };

        foreach (var (endpoint, metrics) in _metrics)
        {
            if (endpoint == "_rate_limit")
            {
                summary.RateLimitRemaining = metrics.RateLimitRemaining;
                summary.RateLimitTotal = metrics.RateLimitTotal;
                summary.RateLimitResetTime = metrics.RateLimitResetTime;
                continue;
            }

            lock (metrics)
            {
                summary.EndpointMetrics[endpoint] = new EndpointSummary
                {
                    TotalCalls = metrics.TotalCalls,
                    SuccessfulCalls = metrics.SuccessfulCalls,
                    FailedCalls = metrics.FailedCalls,
                    SuccessRate = metrics.TotalCalls > 0 ? (double)metrics.SuccessfulCalls / metrics.TotalCalls : 0,
                    AverageDuration = metrics.TotalCalls > 0 ? metrics.TotalDuration.TotalMilliseconds / metrics.TotalCalls : 0,
                    MinDuration = metrics.MinDuration.TotalMilliseconds,
                    MaxDuration = metrics.MaxDuration.TotalMilliseconds,
                    LastCallTime = metrics.LastCallTime,
                    StatusCodeDistribution = new Dictionary<int, int>(metrics.StatusCodes)
                };
            }
        }

        return summary;
    }

    public void LogMetricsSummary()
    {
        var summary = GetSummary();

        _logger.LogInformation("=== Unsplash API Metrics Summary ===");
        _logger.LogInformation("Rate Limit: {Remaining}/{Total} (Reset: {ResetTime})",
            summary.RateLimitRemaining, summary.RateLimitTotal, summary.RateLimitResetTime);

        foreach (var (endpoint, metrics) in summary.EndpointMetrics)
        {
            _logger.LogInformation("Endpoint: {Endpoint}", endpoint);
            _logger.LogInformation("  Calls: {Total} (Success: {Success}, Failed: {Failed}, Rate: {Rate:P2})",
                metrics.TotalCalls, metrics.SuccessfulCalls, metrics.FailedCalls, metrics.SuccessRate);
            _logger.LogInformation("  Duration: Avg {Avg:F1}ms, Min {Min:F1}ms, Max {Max:F1}ms",
                metrics.AverageDuration, metrics.MinDuration, metrics.MaxDuration);
            _logger.LogInformation("  Last Call: {LastCall}", metrics.LastCallTime);
        }
    }
}

public class ApiMetrics
{
    public int TotalCalls { get; set; }
    public int SuccessfulCalls { get; set; }
    public int FailedCalls { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public DateTimeOffset LastCallTime { get; set; }
    public ConcurrentDictionary<int, int> StatusCodes { get; } = new();

    // Rate limit tracking
    public int RateLimitRemaining { get; set; }
    public int RateLimitTotal { get; set; }
    public DateTimeOffset? RateLimitResetTime { get; set; }
}

public class MetricsSummary
{
    public DateTimeOffset GeneratedAt { get; set; }
    public Dictionary<string, EndpointSummary> EndpointMetrics { get; set; } = new();
    public int RateLimitRemaining { get; set; }
    public int RateLimitTotal { get; set; }
    public DateTimeOffset? RateLimitResetTime { get; set; }
}

public class EndpointSummary
{
    public int TotalCalls { get; set; }
    public int SuccessfulCalls { get; set; }
    public int FailedCalls { get; set; }
    public double SuccessRate { get; set; }
    public double AverageDuration { get; set; }
    public double MinDuration { get; set; }
    public double MaxDuration { get; set; }
    public DateTimeOffset LastCallTime { get; set; }
    public Dictionary<int, int> StatusCodeDistribution { get; set; } = new();
}
```
```
```
