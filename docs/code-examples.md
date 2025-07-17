# Code Examples and Recipes

This comprehensive collection provides practical code examples and recipes for common Unsplasharp use cases, from basic operations to advanced integration patterns.

## Table of Contents

- [Basic Operations](#basic-operations)
- [Search and Discovery](#search-and-discovery)
- [User and Collection Management](#user-and-collection-management)
- [Image Processing and Download](#image-processing-and-download)
- [Web Application Integration](#web-application-integration)
- [Desktop Application Examples](#desktop-application-examples)
- [Background Services](#background-services)
- [Testing Patterns](#testing-patterns)
- [Performance Optimization](#performance-optimization)

## Basic Operations

### Simple Photo Retrieval

```csharp
// Get a random photo
public async Task<string> GetRandomPhotoUrl()
{
    var client = new UnsplasharpClient("YOUR_APP_ID");
    
    try
    {
        var photo = await client.GetRandomPhotoAsync();
        return photo.Urls.Regular;
    }
    catch (UnsplasharpException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return "https://via.placeholder.com/800x600?text=Photo+Not+Available";
    }
}

// Get a specific photo with error handling
public async Task<Photo?> GetPhotoSafely(string photoId)
{
    var client = new UnsplasharpClient("YOUR_APP_ID");
    
    try
    {
        return await client.GetPhotoAsync(photoId);
    }
    catch (UnsplasharpNotFoundException)
    {
        Console.WriteLine($"Photo {photoId} not found");
        return null;
    }
    catch (UnsplasharpRateLimitException ex)
    {
        Console.WriteLine($"Rate limited. Try again at {ex.RateLimitReset}");
        return null;
    }
    catch (UnsplasharpException ex)
    {
        Console.WriteLine($"API error: {ex.Message}");
        return null;
    }
}
```

### Photo Information Display

```csharp
public static void DisplayPhotoInfo(Photo photo)
{
    Console.WriteLine("=== PHOTO INFORMATION ===");
    Console.WriteLine($"ID: {photo.Id}");
    Console.WriteLine($"Description: {photo.Description ?? "No description"}");
    Console.WriteLine($"Photographer: {photo.User.Name} (@{photo.User.Username})");
    Console.WriteLine($"Dimensions: {photo.Width}x{photo.Height}");
    Console.WriteLine($"Likes: {photo.Likes:N0} | Downloads: {photo.Downloads:N0}");
    Console.WriteLine($"Color: {photo.Color}");
    Console.WriteLine($"Created: {DateTime.Parse(photo.CreatedAt):yyyy-MM-dd}");
    
    // URLs
    Console.WriteLine("\n=== AVAILABLE SIZES ===");
    Console.WriteLine($"Thumbnail: {photo.Urls.Thumbnail}");
    Console.WriteLine($"Small: {photo.Urls.Small}");
    Console.WriteLine($"Regular: {photo.Urls.Regular}");
    Console.WriteLine($"Full: {photo.Urls.Full}");
    Console.WriteLine($"Raw: {photo.Urls.Raw}");
    
    // Location (if available)
    if (!string.IsNullOrEmpty(photo.Location.Name))
    {
        Console.WriteLine($"\n=== LOCATION ===");
        Console.WriteLine($"Location: {photo.Location.Name}");
        if (photo.Location.Position != null)
        {
            Console.WriteLine($"Coordinates: {photo.Location.Position.Latitude}, {photo.Location.Position.Longitude}");
        }
    }
    
    // Camera info (if available)
    if (!string.IsNullOrEmpty(photo.Exif.Make))
    {
        Console.WriteLine($"\n=== CAMERA INFO ===");
        Console.WriteLine($"Camera: {photo.Exif.Make} {photo.Exif.Model}");
        Console.WriteLine($"Settings: f/{photo.Exif.Aperture}, {photo.Exif.ExposureTime}s, ISO {photo.Exif.Iso}");
        Console.WriteLine($"Focal Length: {photo.Exif.FocalLength}");
    }
}
```

## Search and Discovery

### Smart Search with Fallbacks

```csharp
public class SmartPhotoSearch
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<SmartPhotoSearch> _logger;

    public SmartPhotoSearch(UnsplasharpClient client, ILogger<SmartPhotoSearch> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<Photo>> SearchWithFallbacks(string query, int desiredCount = 20)
    {
        var strategies = new List<(string name, Func<Task<List<Photo>>> search)>
        {
            ("Exact match", () => _client.SearchPhotosAsync(query, perPage: desiredCount)),
            ("Popular results", () => _client.SearchPhotosAsync(query, orderBy: OrderBy.Popular, perPage: desiredCount)),
            ("Broader search", () => SearchBroaderTerms(query, desiredCount)),
            ("Random fallback", () => GetRandomPhotosForQuery(query, desiredCount))
        };

        foreach (var (name, search) in strategies)
        {
            try
            {
                var results = await search();
                if (results.Count > 0)
                {
                    _logger.LogInformation("Search strategy '{Strategy}' succeeded with {Count} results", name, results.Count);
                    return results;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Search strategy '{Strategy}' failed", name);
            }
        }

        _logger.LogWarning("All search strategies failed for query: {Query}", query);
        return new List<Photo>();
    }

    private async Task<List<Photo>> SearchBroaderTerms(string query, int count)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var allPhotos = new List<Photo>();

        foreach (var word in words.Take(3))
        {
            try
            {
                var photos = await _client.SearchPhotosAsync(word, perPage: count / words.Length + 5);
                allPhotos.AddRange(photos);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to search for word: {Word}", word);
            }
        }

        return allPhotos.DistinctBy(p => p.Id).Take(count).ToList();
    }

    private async Task<List<Photo>> GetRandomPhotosForQuery(string query, int count)
    {
        try
        {
            return await _client.GetRandomPhotosAsync(count, query: query);
        }
        catch
        {
            // Final fallback - completely random photos
            return await _client.GetRandomPhotosAsync(count);
        }
    }
}
```

### Advanced Search Filters

```csharp
public class AdvancedPhotoSearch
{
    private readonly UnsplasharpClient _client;

    public AdvancedPhotoSearch(UnsplasharpClient client)
    {
        _client = client;
    }

    // Search for high-quality landscape photos
    public async Task<List<Photo>> GetHighQualityLandscapes(string query, int count = 20)
    {
        var photos = await _client.SearchPhotosAsync(
            query: $"{query} landscape",
            orderBy: OrderBy.Popular,
            orientation: Orientation.Landscape,
            perPage: count * 2 // Get more to filter
        );

        return photos
            .Where(p => p.Width >= 1920 && p.Height >= 1080) // HD or better
            .Where(p => p.Likes >= 100) // Popular photos
            .Take(count)
            .ToList();
    }

    // Search for photos by color theme
    public async Task<List<Photo>> GetPhotosByColor(string query, string color, int count = 20)
    {
        return await _client.SearchPhotosAsync(
            query: query,
            color: color,
            orderBy: OrderBy.Popular,
            perPage: count
        );
    }

    // Search for portrait photos suitable for profiles
    public async Task<List<Photo>> GetPortraitPhotos(string query, int count = 20)
    {
        var photos = await _client.SearchPhotosAsync(
            query: $"{query} portrait person face",
            orientation: Orientation.Portrait,
            contentFilter: "high", // Safe content
            orderBy: OrderBy.Popular,
            perPage: count * 2
        );

        return photos
            .Where(p => p.Height > p.Width) // Ensure portrait orientation
            .Where(p => p.Likes >= 50) // Some popularity
            .Take(count)
            .ToList();
    }

    // Search within specific collections
    public async Task<List<Photo>> SearchInCollections(string query, string[] collectionIds, int count = 20)
    {
        var collectionIdsString = string.Join(",", collectionIds);
        
        return await _client.SearchPhotosAsync(
            query: query,
            collectionIds: collectionIdsString,
            orderBy: OrderBy.Relevant,
            perPage: count
        );
    }
}
```

### Pagination Helper

```csharp
public class PaginatedSearch
{
    private readonly UnsplasharpClient _client;

    public PaginatedSearch(UnsplasharpClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<Photo> SearchAllPhotos(
        string query,
        int batchSize = 30,
        int maxPhotos = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int currentPage = 1;
        int totalReturned = 0;
        bool hasMoreResults = true;

        while (hasMoreResults && totalReturned < maxPhotos && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var photos = await _client.SearchPhotosAsync(
                    query,
                    page: currentPage,
                    perPage: Math.Min(batchSize, maxPhotos - totalReturned),
                    cancellationToken: cancellationToken
                );

                if (photos.Count == 0)
                {
                    hasMoreResults = false;
                    yield break;
                }

                foreach (var photo in photos)
                {
                    if (totalReturned >= maxPhotos)
                        yield break;

                    yield return photo;
                    totalReturned++;
                }

                hasMoreResults = photos.Count == batchSize && 
                               currentPage < _client.LastPhotosSearchTotalPages;
                currentPage++;

                // Rate limiting courtesy delay
                await Task.Delay(100, cancellationToken);
            }
            catch (UnsplasharpRateLimitException ex)
            {
                var delay = ex.TimeUntilReset ?? TimeSpan.FromMinutes(1);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception)
            {
                hasMoreResults = false;
            }
        }
    }
}

// Usage example
public async Task SearchExample()
{
    var search = new PaginatedSearch(_client);
    var photoCount = 0;

    await foreach (var photo in search.SearchAllPhotos("nature", maxPhotos: 100))
    {
        Console.WriteLine($"{++photoCount}: {photo.Description} by {photo.User.Name}");
        
        if (photoCount >= 50) // Process first 50
            break;
    }
}
```

## User and Collection Management

### User Profile Analysis

```csharp
public class UserAnalyzer
{
    private readonly UnsplasharpClient _client;

    public UserAnalyzer(UnsplasharpClient client)
    {
        _client = client;
    }

    public async Task<UserProfile> AnalyzeUser(string username)
    {
        var user = await _client.GetUserAsync(username);
        var userPhotos = await _client.GetUserPhotosAsync(username, perPage: 30);
        var userLikes = await _client.GetUserLikesAsync(username, perPage: 30);

        return new UserProfile
        {
            User = user,
            RecentPhotos = userPhotos,
            RecentLikes = userLikes,
            EngagementRate = CalculateEngagementRate(user),
            AveragePhotoQuality = CalculateAverageQuality(userPhotos),
            PopularityScore = CalculatePopularityScore(user),
            ActivityLevel = DetermineActivityLevel(user, userPhotos)
        };
    }

    private double CalculateEngagementRate(User user)
    {
        return user.TotalPhotos > 0 ? (double)user.TotalLikes / user.TotalPhotos : 0;
    }

    private double CalculateAverageQuality(List<Photo> photos)
    {
        if (!photos.Any()) return 0;

        return photos.Average(p => 
            (p.Likes * 0.4) + 
            (p.Downloads * 0.3) + 
            (p.Width >= 1920 ? 20 : 0) + 
            (p.Height >= 1080 ? 20 : 0)
        );
    }

    private int CalculatePopularityScore(User user)
    {
        var score = 0;
        
        if (user.TotalLikes > 100000) score += 50;
        else if (user.TotalLikes > 10000) score += 30;
        else if (user.TotalLikes > 1000) score += 10;

        if (user.TotalPhotos > 1000) score += 30;
        else if (user.TotalPhotos > 100) score += 20;
        else if (user.TotalPhotos > 10) score += 10;

        if (user.FollowersCount > 10000) score += 20;
        else if (user.FollowersCount > 1000) score += 10;

        return score;
    }

    private ActivityLevel DetermineActivityLevel(User user, List<Photo> recentPhotos)
    {
        if (!recentPhotos.Any()) return ActivityLevel.Inactive;

        var recentPhotoCount = recentPhotos.Count(p => 
            DateTime.Parse(p.CreatedAt) > DateTime.UtcNow.AddDays(-30));

        return recentPhotoCount switch
        {
            >= 10 => ActivityLevel.VeryActive,
            >= 5 => ActivityLevel.Active,
            >= 1 => ActivityLevel.Moderate,
            _ => ActivityLevel.Low
        };
    }
}

public class UserProfile
{
    public User User { get; set; }
    public List<Photo> RecentPhotos { get; set; } = new();
    public List<Photo> RecentLikes { get; set; } = new();
    public double EngagementRate { get; set; }
    public double AveragePhotoQuality { get; set; }
    public int PopularityScore { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
}

public enum ActivityLevel
{
    Inactive,
    Low,
    Moderate,
    Active,
    VeryActive
}
```

### Collection Explorer

```csharp
public class CollectionExplorer
{
    private readonly UnsplasharpClient _client;

    public CollectionExplorer(UnsplasharpClient client)
    {
        _client = client;
    }

    public async Task<CollectionAnalysis> AnalyzeCollection(string collectionId)
    {
        var collection = await _client.GetCollectionAsync(collectionId);
        var photos = await _client.GetCollectionPhotosAsync(collectionId, perPage: 30);

        var analysis = new CollectionAnalysis
        {
            Collection = collection,
            SamplePhotos = photos,
            AveragePhotoQuality = photos.Any() ? photos.Average(p => p.Likes + p.Downloads) : 0,
            DominantColors = GetDominantColors(photos),
            CommonOrientations = GetOrientationDistribution(photos),
            TopPhotographers = GetTopPhotographers(photos),
            QualityScore = CalculateCollectionQuality(collection, photos)
        };

        return analysis;
    }

    private Dictionary<string, int> GetDominantColors(List<Photo> photos)
    {
        return photos
            .GroupBy(p => p.Color)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private Dictionary<string, int> GetOrientationDistribution(List<Photo> photos)
    {
        return photos
            .GroupBy(p => p.Width > p.Height ? "Landscape" : 
                         p.Height > p.Width ? "Portrait" : "Square")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private List<(string Name, int PhotoCount)> GetTopPhotographers(List<Photo> photos)
    {
        return photos
            .GroupBy(p => p.User.Name)
            .Select(g => (Name: g.Key, PhotoCount: g.Count()))
            .OrderByDescending(x => x.PhotoCount)
            .Take(5)
            .ToList();
    }

    private int CalculateCollectionQuality(Collection collection, List<Photo> photos)
    {
        var score = 0;
        
        // Collection size scoring
        if (collection.TotalPhotos > 100) score += 20;
        else if (collection.TotalPhotos > 50) score += 15;
        else if (collection.TotalPhotos > 10) score += 10;

        // Photo quality scoring
        if (photos.Any())
        {
            var avgLikes = photos.Average(p => p.Likes);
            if (avgLikes > 1000) score += 30;
            else if (avgLikes > 100) score += 20;
            else if (avgLikes > 10) score += 10;

            var highResCount = photos.Count(p => p.Width >= 1920 && p.Height >= 1080);
            score += (highResCount * 100 / photos.Count) / 5; // Up to 20 points
        }

        // Curator reputation
        var curator = collection.User;
        if (curator.TotalLikes > 10000) score += 15;
        else if (curator.TotalLikes > 1000) score += 10;

        return Math.Min(score, 100); // Cap at 100
    }
}

public class CollectionAnalysis
{
    public Collection Collection { get; set; }
    public List<Photo> SamplePhotos { get; set; } = new();
    public double AveragePhotoQuality { get; set; }
    public Dictionary<string, int> DominantColors { get; set; } = new();
    public Dictionary<string, int> CommonOrientations { get; set; } = new();
    public List<(string Name, int PhotoCount)> TopPhotographers { get; set; } = new();
    public int QualityScore { get; set; }
}
```

## Image Processing and Download

### Smart Image Downloader

```csharp
public class SmartImageDownloader
{
    private readonly UnsplasharpClient _client;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SmartImageDownloader> _logger;

    public SmartImageDownloader(UnsplasharpClient client, HttpClient httpClient, ILogger<SmartImageDownloader> logger)
    {
        _client = client;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DownloadResult> DownloadOptimalSize(string photoId, int maxWidth, int maxHeight, string downloadPath)
    {
        try
        {
            var photo = await _client.GetPhotoAsync(photoId);
            var optimalUrl = SelectOptimalUrl(photo.Urls, maxWidth, maxHeight);

            _logger.LogInformation("Downloading photo {PhotoId} from {Url}", photoId, optimalUrl);

            var imageBytes = await _httpClient.GetByteArrayAsync(optimalUrl);
            var fileName = GenerateFileName(photo, optimalUrl);
            var fullPath = Path.Combine(downloadPath, fileName);

            Directory.CreateDirectory(downloadPath);
            await File.WriteAllBytesAsync(fullPath, imageBytes);

            return new DownloadResult
            {
                Success = true,
                FilePath = fullPath,
                FileSize = imageBytes.Length,
                Photo = photo,
                DownloadUrl = optimalUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download photo {PhotoId}", photoId);
            return new DownloadResult { Success = false, Error = ex.Message };
        }
    }

    private string SelectOptimalUrl(Urls urls, int maxWidth, int maxHeight)
    {
        var maxDimension = Math.Max(maxWidth, maxHeight);

        return maxDimension switch
        {
            <= 200 => urls.Thumbnail,
            <= 400 => urls.Small,
            <= 1080 => urls.Regular,
            <= 2048 => urls.Full,
            _ => urls.Raw
        };
    }

    private string GenerateFileName(Photo photo, string url)
    {
        var extension = url.Contains("fm=") && url.Contains("fm=webp") ? "webp" : "jpg";
        var sanitizedDescription = SanitizeFileName(photo.Description ?? "untitled");
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        return $"{photo.Id}_{sanitizedDescription}_{timestamp}.{extension}";
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        return sanitized.Length > 50 ? sanitized.Substring(0, 50) : sanitized;
    }

    public async Task<BatchDownloadResult> DownloadMultiplePhotos(
        IEnumerable<string> photoIds,
        string downloadPath,
        int maxWidth = 1920,
        int maxHeight = 1080,
        int maxConcurrency = 3)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var result = new BatchDownloadResult();

        var downloadTasks = photoIds.Select(async photoId =>
        {
            await semaphore.WaitAsync();
            try
            {
                var downloadResult = await DownloadOptimalSize(photoId, maxWidth, maxHeight, downloadPath);

                lock (result)
                {
                    if (downloadResult.Success)
                        result.SuccessfulDownloads.Add(downloadResult);
                    else
                        result.FailedDownloads.Add(photoId, downloadResult.Error ?? "Unknown error");
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(downloadTasks);
        return result;
    }
}

public class DownloadResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public Photo? Photo { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Error { get; set; }
}

public class BatchDownloadResult
{
    public List<DownloadResult> SuccessfulDownloads { get; } = new();
    public Dictionary<string, string> FailedDownloads { get; } = new();

    public int TotalAttempted => SuccessfulDownloads.Count + FailedDownloads.Count;
    public double SuccessRate => TotalAttempted > 0 ? (double)SuccessfulDownloads.Count / TotalAttempted : 0;
}
```

### Image Metadata Extractor

```csharp
public class ImageMetadataExtractor
{
    public static ImageMetadata ExtractMetadata(Photo photo)
    {
        return new ImageMetadata
        {
            Id = photo.Id,
            Title = photo.Description ?? "Untitled",
            Photographer = photo.User.Name,
            PhotographerUsername = photo.User.Username,
            Dimensions = new Size(photo.Width, photo.Height),
            AspectRatio = (double)photo.Width / photo.Height,
            DominantColor = photo.Color,
            BlurHash = photo.BlurHash,
            CreatedAt = DateTime.Parse(photo.CreatedAt),
            Engagement = new EngagementMetrics
            {
                Likes = photo.Likes,
                Downloads = photo.Downloads,
                IsLikedByUser = photo.IsLikedByUser
            },
            Camera = ExtractCameraInfo(photo.Exif),
            Location = ExtractLocationInfo(photo.Location),
            Keywords = ExtractKeywords(photo),
            QualityScore = CalculateQualityScore(photo)
        };
    }

    private static CameraInfo? ExtractCameraInfo(Exif exif)
    {
        if (string.IsNullOrEmpty(exif.Make)) return null;

        return new CameraInfo
        {
            Make = exif.Make,
            Model = exif.Model,
            Aperture = exif.Aperture,
            ExposureTime = exif.ExposureTime,
            Iso = exif.Iso,
            FocalLength = exif.FocalLength
        };
    }

    private static LocationInfo? ExtractLocationInfo(Location location)
    {
        if (string.IsNullOrEmpty(location.Name)) return null;

        return new LocationInfo
        {
            Name = location.Name,
            City = location.City,
            Country = location.Country,
            Coordinates = location.Position != null
                ? new Coordinates(location.Position.Latitude, location.Position.Longitude)
                : null
        };
    }

    private static List<string> ExtractKeywords(Photo photo)
    {
        var keywords = new List<string>();

        // Add categories
        keywords.AddRange(photo.Categories.Select(c => c.Title));

        // Extract from description
        if (!string.IsNullOrEmpty(photo.Description))
        {
            var words = photo.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .Select(w => w.Trim('.', ',', '!', '?').ToLowerInvariant())
                .Distinct();
            keywords.AddRange(words);
        }

        // Add orientation
        keywords.Add(photo.Width > photo.Height ? "landscape" :
                    photo.Height > photo.Width ? "portrait" : "square");

        // Add color
        keywords.Add($"color-{photo.Color.TrimStart('#')}");

        return keywords.Distinct().ToList();
    }

    private static int CalculateQualityScore(Photo photo)
    {
        var score = 0;

        // Resolution scoring
        var pixels = photo.Width * photo.Height;
        if (pixels >= 8000000) score += 25; // 8MP+
        else if (pixels >= 2000000) score += 20; // 2MP+
        else if (pixels >= 1000000) score += 15; // 1MP+
        else score += 10;

        // Engagement scoring
        if (photo.Likes > 10000) score += 25;
        else if (photo.Likes > 1000) score += 20;
        else if (photo.Likes > 100) score += 15;
        else score += 10;

        // Technical quality
        if (!string.IsNullOrEmpty(photo.Exif.Make)) score += 15; // Has EXIF
        if (!string.IsNullOrEmpty(photo.Location.Name)) score += 10; // Has location
        if (!string.IsNullOrEmpty(photo.Description)) score += 10; // Has description

        // Photographer reputation
        if (photo.User.TotalLikes > 100000) score += 15;
        else if (photo.User.TotalLikes > 10000) score += 10;
        else if (photo.User.TotalLikes > 1000) score += 5;

        return Math.Min(score, 100);
    }
}

public class ImageMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Photographer { get; set; } = string.Empty;
    public string PhotographerUsername { get; set; } = string.Empty;
    public Size Dimensions { get; set; }
    public double AspectRatio { get; set; }
    public string DominantColor { get; set; } = string.Empty;
    public string BlurHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public EngagementMetrics Engagement { get; set; } = new();
    public CameraInfo? Camera { get; set; }
    public LocationInfo? Location { get; set; }
    public List<string> Keywords { get; set; } = new();
    public int QualityScore { get; set; }
}

public record Size(int Width, int Height);
public record Coordinates(double Latitude, double Longitude);

public class EngagementMetrics
{
    public int Likes { get; set; }
    public int Downloads { get; set; }
    public bool IsLikedByUser { get; set; }
}

public class CameraInfo
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Aperture { get; set; } = string.Empty;
    public string ExposureTime { get; set; } = string.Empty;
    public int Iso { get; set; }
    public string FocalLength { get; set; } = string.Empty;
}

public class LocationInfo
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Coordinates? Coordinates { get; set; }
}
```

## Web Application Integration

### ASP.NET Core Photo API

```csharp
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly UnsplasharpClient _unsplashClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PhotosController> _logger;

    public PhotosController(UnsplasharpClient unsplashClient, IMemoryCache cache, ILogger<PhotosController> logger)
    {
        _unsplashClient = unsplashClient;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("random")]
    public async Task<ActionResult<PhotoDto>> GetRandomPhoto([FromQuery] string? query = null)
    {
        try
        {
            var photo = string.IsNullOrEmpty(query)
                ? await _unsplashClient.GetRandomPhotoAsync()
                : await _unsplashClient.GetRandomPhotoAsync(query: query);

            return Ok(PhotoDto.FromPhoto(photo));
        }
        catch (UnsplasharpRateLimitException ex)
        {
            _logger.LogWarning("Rate limit exceeded: {RemainingRequests}/{TotalRequests}",
                ex.RateLimitRemaining, ex.RateLimit);
            return StatusCode(429, new { error = "Rate limit exceeded", retryAfter = ex.TimeUntilReset });
        }
        catch (UnsplasharpException ex)
        {
            _logger.LogError(ex, "Error getting random photo");
            return StatusCode(500, new { error = "Failed to fetch photo" });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<SearchResultDto>> SearchPhotos(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20,
        [FromQuery] string? color = null,
        [FromQuery] string? orientation = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Query parameter is required" });

        var cacheKey = $"search:{query}:{page}:{perPage}:{color}:{orientation}";

        if (_cache.TryGetValue(cacheKey, out SearchResultDto cachedResult))
        {
            return Ok(cachedResult);
        }

        try
        {
            var orientationEnum = orientation?.ToLowerInvariant() switch
            {
                "landscape" => Orientation.Landscape,
                "portrait" => Orientation.Portrait,
                "squarish" => Orientation.Squarish,
                _ => Orientation.All
            };

            var photos = await _unsplashClient.SearchPhotosAsync(
                query: query,
                page: page,
                perPage: Math.Min(perPage, 30), // Limit max per page
                color: color,
                orientation: orientationEnum
            );

            var result = new SearchResultDto
            {
                Photos = photos.Select(PhotoDto.FromPhoto).ToList(),
                TotalResults = _unsplashClient.LastPhotosSearchTotalResults,
                TotalPages = _unsplashClient.LastPhotosSearchTotalPages,
                CurrentPage = page,
                Query = query
            };

            // Cache for 5 minutes
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return Ok(result);
        }
        catch (UnsplasharpException ex)
        {
            _logger.LogError(ex, "Error searching photos with query: {Query}", query);
            return StatusCode(500, new { error = "Search failed" });
        }
    }

    [HttpGet("{photoId}")]
    public async Task<ActionResult<PhotoDto>> GetPhoto(string photoId)
    {
        var cacheKey = $"photo:{photoId}";

        if (_cache.TryGetValue(cacheKey, out PhotoDto cachedPhoto))
        {
            return Ok(cachedPhoto);
        }

        try
        {
            var photo = await _unsplashClient.GetPhotoAsync(photoId);
            var photoDto = PhotoDto.FromPhoto(photo);

            // Cache for 1 hour
            _cache.Set(cacheKey, photoDto, TimeSpan.FromHours(1));

            return Ok(photoDto);
        }
        catch (UnsplasharpNotFoundException)
        {
            return NotFound(new { error = $"Photo with ID '{photoId}' not found" });
        }
        catch (UnsplasharpException ex)
        {
            _logger.LogError(ex, "Error getting photo: {PhotoId}", photoId);
            return StatusCode(500, new { error = "Failed to fetch photo" });
        }
    }
}

public class PhotoDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotographerName { get; set; } = string.Empty;
    public string PhotographerUsername { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Downloads { get; set; }
    public DateTime CreatedAt { get; set; }
    public UrlsDto Urls { get; set; } = new();

    public static PhotoDto FromPhoto(Photo photo)
    {
        return new PhotoDto
        {
            Id = photo.Id,
            Description = photo.Description ?? string.Empty,
            PhotographerName = photo.User.Name,
            PhotographerUsername = photo.User.Username,
            Width = photo.Width,
            Height = photo.Height,
            Color = photo.Color,
            Likes = photo.Likes,
            Downloads = photo.Downloads,
            CreatedAt = DateTime.Parse(photo.CreatedAt),
            Urls = new UrlsDto
            {
                Thumbnail = photo.Urls.Thumbnail,
                Small = photo.Urls.Small,
                Regular = photo.Urls.Regular,
                Full = photo.Urls.Full
            }
        };
    }
}

public class UrlsDto
{
    public string Thumbnail { get; set; } = string.Empty;
    public string Small { get; set; } = string.Empty;
    public string Regular { get; set; } = string.Empty;
    public string Full { get; set; } = string.Empty;
}

public class SearchResultDto
{
    public List<PhotoDto> Photos { get; set; } = new();
    public int TotalResults { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public string Query { get; set; } = string.Empty;
}
```

## Desktop Application Examples

### WPF Photo Gallery

```csharp
public partial class PhotoGalleryWindow : Window
{
    private readonly UnsplasharpClient _client;
    private readonly ObservableCollection<PhotoViewModel> _photos;
    private CancellationTokenSource? _searchCancellation;

    public PhotoGalleryWindow()
    {
        InitializeComponent();
        _client = new UnsplasharpClient("YOUR_APP_ID");
        _photos = new ObservableCollection<PhotoViewModel>();
        PhotosListView.ItemsSource = _photos;
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var query = SearchTextBox.Text.Trim();
        if (string.IsNullOrEmpty(query)) return;

        // Cancel previous search
        _searchCancellation?.Cancel();
        _searchCancellation = new CancellationTokenSource();

        LoadingProgressBar.Visibility = Visibility.Visible;
        SearchButton.IsEnabled = false;
        _photos.Clear();

        try
        {
            var photos = await _client.SearchPhotosAsync(query, perPage: 30, cancellationToken: _searchCancellation.Token);

            foreach (var photo in photos)
            {
                _photos.Add(new PhotoViewModel(photo));
            }

            StatusTextBlock.Text = $"Found {_client.LastPhotosSearchTotalResults:N0} photos";
        }
        catch (OperationCanceledException)
        {
            StatusTextBlock.Text = "Search cancelled";
        }
        catch (UnsplasharpRateLimitException ex)
        {
            StatusTextBlock.Text = $"Rate limited. Try again at {ex.RateLimitReset:HH:mm}";
        }
        catch (UnsplasharpException ex)
        {
            StatusTextBlock.Text = $"Error: {ex.Message}";
        }
        finally
        {
            LoadingProgressBar.Visibility = Visibility.Collapsed;
            SearchButton.IsEnabled = true;
        }
    }

    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is PhotoViewModel photoVM)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"{photoVM.Id}_{photoVM.PhotographerName}.jpg",
                Filter = "JPEG Image|*.jpg|All Files|*.*"
            };

            if (saveDialog.ShowDialog() == true)
            {
                button.IsEnabled = false;
                button.Content = "Downloading...";

                try
                {
                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(photoVM.RegularUrl);
                    await File.WriteAllBytesAsync(saveDialog.FileName, imageBytes);

                    MessageBox.Show("Photo downloaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Download failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    button.Content = "Download";
                    button.IsEnabled = true;
                }
            }
        }
    }

    private void RandomPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadRandomPhoto();
    }

    private async Task LoadRandomPhoto()
    {
        LoadingProgressBar.Visibility = Visibility.Visible;

        try
        {
            var photo = await _client.GetRandomPhotoAsync();
            _photos.Clear();
            _photos.Add(new PhotoViewModel(photo));
            StatusTextBlock.Text = "Random photo loaded";
        }
        catch (UnsplasharpException ex)
        {
            StatusTextBlock.Text = $"Error: {ex.Message}";
        }
        finally
        {
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }
    }
}

public class PhotoViewModel : INotifyPropertyChanged
{
    public string Id { get; }
    public string Description { get; }
    public string PhotographerName { get; }
    public string PhotographerUsername { get; }
    public int Width { get; }
    public int Height { get; }
    public string Color { get; }
    public int Likes { get; }
    public string ThumbnailUrl { get; }
    public string SmallUrl { get; }
    public string RegularUrl { get; }
    public string AspectRatioText { get; }
    public string DimensionsText { get; }
    public string EngagementText { get; }

    public PhotoViewModel(Photo photo)
    {
        Id = photo.Id;
        Description = photo.Description ?? "Untitled";
        PhotographerName = photo.User.Name;
        PhotographerUsername = photo.User.Username;
        Width = photo.Width;
        Height = photo.Height;
        Color = photo.Color;
        Likes = photo.Likes;
        ThumbnailUrl = photo.Urls.Thumbnail;
        SmallUrl = photo.Urls.Small;
        RegularUrl = photo.Urls.Regular;

        AspectRatioText = $"{(double)Width / Height:F2}:1";
        DimensionsText = $"{Width}×{Height}";
        EngagementText = $"{Likes:N0} likes";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
```

### Console Photo Browser

```csharp
public class ConsolePhotoBrowser
{
    private readonly UnsplasharpClient _client;
    private readonly List<Photo> _currentPhotos = new();
    private int _currentIndex = 0;

    public ConsolePhotoBrowser(string applicationId)
    {
        _client = new UnsplasharpClient(applicationId);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== Unsplash Photo Browser ===");
        Console.WriteLine("Commands: search <query>, random, next, prev, info, download, quit");
        Console.WriteLine();

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(input)) continue;

            var parts = input.Split(' ', 2);
            var command = parts[0];
            var argument = parts.Length > 1 ? parts[1] : string.Empty;

            try
            {
                switch (command)
                {
                    case "search":
                        if (string.IsNullOrEmpty(argument))
                        {
                            Console.WriteLine("Usage: search <query>");
                            break;
                        }
                        await SearchPhotos(argument);
                        break;

                    case "random":
                        await LoadRandomPhoto();
                        break;

                    case "next":
                        ShowNextPhoto();
                        break;

                    case "prev":
                        ShowPreviousPhoto();
                        break;

                    case "info":
                        ShowCurrentPhotoInfo();
                        break;

                    case "download":
                        await DownloadCurrentPhoto();
                        break;

                    case "quit":
                    case "exit":
                        return;

                    default:
                        Console.WriteLine("Unknown command. Available: search, random, next, prev, info, download, quit");
                        break;
                }
            }
            catch (UnsplasharpRateLimitException ex)
            {
                Console.WriteLine($"Rate limited. Try again at {ex.RateLimitReset:HH:mm:ss}");
            }
            catch (UnsplasharpException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    private async Task SearchPhotos(string query)
    {
        Console.WriteLine($"Searching for '{query}'...");

        var photos = await _client.SearchPhotosAsync(query, perPage: 20);
        _currentPhotos.Clear();
        _currentPhotos.AddRange(photos);
        _currentIndex = 0;

        Console.WriteLine($"Found {_client.LastPhotosSearchTotalResults:N0} photos ({photos.Count} loaded)");

        if (photos.Count > 0)
        {
            ShowCurrentPhoto();
        }
    }

    private async Task LoadRandomPhoto()
    {
        Console.WriteLine("Loading random photo...");

        var photo = await _client.GetRandomPhotoAsync();
        _currentPhotos.Clear();
        _currentPhotos.Add(photo);
        _currentIndex = 0;

        ShowCurrentPhoto();
    }

    private void ShowNextPhoto()
    {
        if (_currentPhotos.Count == 0)
        {
            Console.WriteLine("No photos loaded. Use 'search' or 'random' first.");
            return;
        }

        _currentIndex = (_currentIndex + 1) % _currentPhotos.Count;
        ShowCurrentPhoto();
    }

    private void ShowPreviousPhoto()
    {
        if (_currentPhotos.Count == 0)
        {
            Console.WriteLine("No photos loaded. Use 'search' or 'random' first.");
            return;
        }

        _currentIndex = _currentIndex == 0 ? _currentPhotos.Count - 1 : _currentIndex - 1;
        ShowCurrentPhoto();
    }

    private void ShowCurrentPhoto()
    {
        if (_currentPhotos.Count == 0) return;

        var photo = _currentPhotos[_currentIndex];

        Console.WriteLine($"Photo {_currentIndex + 1}/{_currentPhotos.Count}:");
        Console.WriteLine($"  ID: {photo.Id}");
        Console.WriteLine($"  Title: {photo.Description ?? "Untitled"}");
        Console.WriteLine($"  By: {photo.User.Name} (@{photo.User.Username})");
        Console.WriteLine($"  Size: {photo.Width}×{photo.Height}");
        Console.WriteLine($"  Likes: {photo.Likes:N0}");
        Console.WriteLine($"  URL: {photo.Urls.Regular}");
    }

    private void ShowCurrentPhotoInfo()
    {
        if (_currentPhotos.Count == 0)
        {
            Console.WriteLine("No photo selected.");
            return;
        }

        var photo = _currentPhotos[_currentIndex];
        DisplayPhotoInfo(photo); // Use the method from earlier examples
    }

    private async Task DownloadCurrentPhoto()
    {
        if (_currentPhotos.Count == 0)
        {
            Console.WriteLine("No photo selected.");
            return;
        }

        var photo = _currentPhotos[_currentIndex];
        var fileName = $"{photo.Id}_{photo.User.Username}.jpg";

        Console.WriteLine($"Downloading {fileName}...");

        try
        {
            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(photo.Urls.Regular);
            await File.WriteAllBytesAsync(fileName, imageBytes);

            Console.WriteLine($"Downloaded to {fileName} ({imageBytes.Length:N0} bytes)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
        }
    }
}

// Usage
class Program
{
    static async Task Main(string[] args)
    {
        var browser = new ConsolePhotoBrowser("YOUR_APP_ID");
        await browser.RunAsync();
    }
}
```

## Background Services

### Photo Sync Service

```csharp
public class PhotoSyncService : BackgroundService
{
    private readonly UnsplasharpClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PhotoSyncService> _logger;
    private readonly PhotoSyncOptions _options;

    public PhotoSyncService(
        UnsplasharpClient client,
        IServiceProvider serviceProvider,
        ILogger<PhotoSyncService> logger,
        IOptions<PhotoSyncOptions> options)
    {
        _client = client;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Photo sync service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncPhotos(stoppingToken);
                await Task.Delay(_options.SyncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during photo sync");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retry
            }
        }

        _logger.LogInformation("Photo sync service stopped");
    }

    private async Task SyncPhotos(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var photoRepository = scope.ServiceProvider.GetRequiredService<IPhotoRepository>();

        foreach (var query in _options.SyncQueries)
        {
            try
            {
                _logger.LogInformation("Syncing photos for query: {Query}", query);

                var photos = await _client.SearchPhotosAsync(query, perPage: _options.PhotosPerQuery, cancellationToken: cancellationToken);

                foreach (var photo in photos)
                {
                    await SyncSinglePhoto(photo, photoRepository, cancellationToken);
                }

                _logger.LogInformation("Synced {Count} photos for query: {Query}", photos.Count, query);
            }
            catch (UnsplasharpRateLimitException ex)
            {
                _logger.LogWarning("Rate limited during sync, waiting {Delay}ms", ex.TimeUntilReset?.TotalMilliseconds);

                if (ex.TimeUntilReset.HasValue)
                {
                    await Task.Delay(ex.TimeUntilReset.Value, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing photos for query: {Query}", query);
            }
        }
    }

    private async Task SyncSinglePhoto(Photo photo, IPhotoRepository repository, CancellationToken cancellationToken)
    {
        try
        {
            var existingPhoto = await repository.GetByIdAsync(photo.Id);

            if (existingPhoto == null)
            {
                // New photo
                var photoEntity = MapToEntity(photo);
                await repository.AddAsync(photoEntity);
                _logger.LogDebug("Added new photo: {PhotoId}", photo.Id);
            }
            else if (existingPhoto.UpdatedAt < DateTime.Parse(photo.UpdatedAt))
            {
                // Updated photo
                var updatedEntity = MapToEntity(photo);
                await repository.UpdateAsync(updatedEntity);
                _logger.LogDebug("Updated photo: {PhotoId}", photo.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing photo: {PhotoId}", photo.Id);
        }
    }

    private PhotoEntity MapToEntity(Photo photo)
    {
        return new PhotoEntity
        {
            Id = photo.Id,
            Description = photo.Description,
            Width = photo.Width,
            Height = photo.Height,
            Color = photo.Color,
            Likes = photo.Likes,
            Downloads = photo.Downloads,
            CreatedAt = DateTime.Parse(photo.CreatedAt),
            UpdatedAt = DateTime.Parse(photo.UpdatedAt),
            PhotographerName = photo.User.Name,
            PhotographerUsername = photo.User.Username,
            ThumbnailUrl = photo.Urls.Thumbnail,
            SmallUrl = photo.Urls.Small,
            RegularUrl = photo.Urls.Regular,
            FullUrl = photo.Urls.Full,
            LastSyncedAt = DateTime.UtcNow
        };
    }
}

public class PhotoSyncOptions
{
    public TimeSpan SyncInterval { get; set; } = TimeSpan.FromHours(1);
    public List<string> SyncQueries { get; set; } = new() { "nature", "technology", "business" };
    public int PhotosPerQuery { get; set; } = 30;
}

public interface IPhotoRepository
{
    Task<PhotoEntity?> GetByIdAsync(string id);
    Task AddAsync(PhotoEntity photo);
    Task UpdateAsync(PhotoEntity photo);
}

public class PhotoEntity
{
    public string Id { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Downloads { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string PhotographerName { get; set; } = string.Empty;
    public string PhotographerUsername { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string SmallUrl { get; set; } = string.Empty;
    public string RegularUrl { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public DateTime LastSyncedAt { get; set; }
}

// Registration in Program.cs
services.Configure<PhotoSyncOptions>(configuration.GetSection("PhotoSync"));
services.AddHostedService<PhotoSyncService>();
```

## Testing Patterns

### Unit Testing with Mocking

```csharp
[TestFixture]
public class PhotoServiceTests
{
    private Mock<UnsplasharpClient> _mockClient;
    private Mock<ILogger<PhotoService>> _mockLogger;
    private PhotoService _photoService;

    [SetUp]
    public void Setup()
    {
        _mockClient = new Mock<UnsplasharpClient>("test-app-id");
        _mockLogger = new Mock<ILogger<PhotoService>>();
        _photoService = new PhotoService(_mockClient.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetRandomPhoto_Success_ReturnsPhoto()
    {
        // Arrange
        var expectedPhoto = CreateTestPhoto();
        _mockClient.Setup(c => c.GetRandomPhotoAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(expectedPhoto);

        // Act
        var result = await _photoService.GetRandomPhotoAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedPhoto.Id, result.Id);
        Assert.AreEqual(expectedPhoto.Description, result.Description);
    }

    [Test]
    public async Task GetRandomPhoto_RateLimited_ReturnsNull()
    {
        // Arrange
        _mockClient.Setup(c => c.GetRandomPhotoAsync(It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new UnsplasharpRateLimitException("Rate limited", null, null, null, null, null));

        // Act
        var result = await _photoService.GetRandomPhotoAsync();

        // Assert
        Assert.IsNull(result);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rate limited")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task SearchPhotos_WithValidQuery_ReturnsPhotos()
    {
        // Arrange
        var query = "nature";
        var expectedPhotos = new List<Photo> { CreateTestPhoto(), CreateTestPhoto() };

        _mockClient.Setup(c => c.SearchPhotosAsync(query, It.IsAny<int>(), It.IsAny<int>(),
                                                  It.IsAny<OrderBy>(), It.IsAny<string>(),
                                                  It.IsAny<string>(), It.IsAny<string>(),
                                                  It.IsAny<Orientation>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(expectedPhotos);

        // Act
        var result = await _photoService.SearchPhotosAsync(query);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedPhotos.Count, result.Count);
    }

    private Photo CreateTestPhoto()
    {
        return new Photo
        {
            Id = Guid.NewGuid().ToString(),
            Description = "Test photo",
            Width = 1920,
            Height = 1080,
            Color = "#FF5733",
            Likes = 100,
            Downloads = 500,
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O"),
            User = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Photographer",
                Username = "testuser"
            },
            Urls = new Urls
            {
                Thumbnail = "https://example.com/thumb.jpg",
                Small = "https://example.com/small.jpg",
                Regular = "https://example.com/regular.jpg",
                Full = "https://example.com/full.jpg",
                Raw = "https://example.com/raw.jpg"
            }
        };
    }
}

public class PhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(UnsplasharpClient client, ILogger<PhotoService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Photo?> GetRandomPhotoAsync()
    {
        try
        {
            return await _client.GetRandomPhotoAsync();
        }
        catch (UnsplasharpRateLimitException ex)
        {
            _logger.LogWarning("Rate limited: {Message}", ex.Message);
            return null;
        }
        catch (UnsplasharpException ex)
        {
            _logger.LogError(ex, "Error getting random photo");
            throw;
        }
    }

    public async Task<List<Photo>> SearchPhotosAsync(string query)
    {
        try
        {
            return await _client.SearchPhotosAsync(query);
        }
        catch (UnsplasharpException ex)
        {
            _logger.LogError(ex, "Error searching photos with query: {Query}", query);
            throw;
        }
    }
}
```

### Integration Testing

```csharp
[TestFixture]
public class UnsplashIntegrationTests
{
    private UnsplasharpClient _client;
    private readonly string _testApplicationId = Environment.GetEnvironmentVariable("UNSPLASH_APP_ID") ?? "test-app-id";

    [SetUp]
    public void Setup()
    {
        _client = new UnsplasharpClient(_testApplicationId);
    }

    [Test]
    [Category("Integration")]
    public async Task GetRandomPhoto_ReturnsValidPhoto()
    {
        // Act
        var photo = await _client.GetRandomPhotoAsync();

        // Assert
        Assert.IsNotNull(photo);
        Assert.IsNotEmpty(photo.Id);
        Assert.IsNotNull(photo.User);
        Assert.IsNotEmpty(photo.User.Name);
        Assert.IsNotNull(photo.Urls);
        Assert.IsNotEmpty(photo.Urls.Regular);
        Assert.Greater(photo.Width, 0);
        Assert.Greater(photo.Height, 0);
    }

    [Test]
    [Category("Integration")]
    public async Task SearchPhotos_WithValidQuery_ReturnsResults()
    {
        // Arrange
        var query = "nature";

        // Act
        var photos = await _client.SearchPhotosAsync(query, perPage: 5);

        // Assert
        Assert.IsNotNull(photos);
        Assert.IsNotEmpty(photos);
        Assert.LessOrEqual(photos.Count, 5);

        foreach (var photo in photos)
        {
            Assert.IsNotEmpty(photo.Id);
            Assert.IsNotNull(photo.User);
            Assert.IsNotNull(photo.Urls);
        }
    }

    [Test]
    [Category("Integration")]
    public async Task GetPhoto_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var invalidId = "invalid-photo-id-12345";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnsplasharpNotFoundException>(
            () => _client.GetPhotoAsync(invalidId));

        Assert.IsNotNull(ex.Context);
        Assert.AreEqual(invalidId, ex.ResourceId);
    }

    [Test]
    [Category("Integration")]
    [Retry(3)] // Retry in case of rate limiting
    public async Task RateLimitHandling_MultipleRequests_HandlesGracefully()
    {
        var tasks = new List<Task<Photo>>();

        // Create multiple concurrent requests
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetRandomPhotoAsync());
        }

        // Some requests might fail due to rate limiting, but shouldn't crash
        var results = await Task.WhenAll(tasks.Select(async task =>
        {
            try
            {
                return await task;
            }
            catch (UnsplasharpRateLimitException)
            {
                return null; // Expected for some requests
            }
        }));

        // At least some requests should succeed
        var successfulResults = results.Where(r => r != null).ToList();
        Assert.Greater(successfulResults.Count, 0);
    }
}
```

## Performance Optimization

### Caching Strategy

```csharp
public class OptimizedPhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<OptimizedPhotoService> _logger;
    private readonly SemaphoreSlim _semaphore;

    public OptimizedPhotoService(
        UnsplasharpClient client,
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<OptimizedPhotoService> logger)
    {
        _client = client;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _semaphore = new SemaphoreSlim(5, 5); // Limit concurrent requests
    }

    public async Task<Photo?> GetPhotoOptimizedAsync(string photoId)
    {
        var cacheKey = $"photo:{photoId}";

        // Try memory cache first (fastest)
        if (_memoryCache.TryGetValue(cacheKey, out Photo cachedPhoto))
        {
            _logger.LogDebug("Photo {PhotoId} found in memory cache", photoId);
            return cachedPhoto;
        }

        // Try distributed cache
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

        // Fetch from API with concurrency control
        await _semaphore.WaitAsync();
        try
        {
            var photo = await _client.GetPhotoAsync(photoId);

            // Cache with intelligent TTL
            var memoryCacheDuration = CalculateCacheDuration(photo);
            _memoryCache.Set(cacheKey, photo, memoryCacheDuration);

            // Store in distributed cache
            var serializedPhoto = JsonSerializer.Serialize(photo);
            await _distributedCache.SetStringAsync(cacheKey, serializedPhoto,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = memoryCacheDuration.Multiply(4)
                });

            _logger.LogDebug("Photo {PhotoId} fetched from API and cached", photoId);
            return photo;
        }
        catch (UnsplasharpNotFoundException)
        {
            // Cache negative results
            _memoryCache.Set(cacheKey, (Photo?)null, TimeSpan.FromMinutes(5));
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<Photo>> SearchPhotosOptimizedAsync(string query, int page = 1, int perPage = 20)
    {
        var cacheKey = $"search:{query}:{page}:{perPage}";

        // Check cache first
        if (_memoryCache.TryGetValue(cacheKey, out List<Photo> cachedResults))
        {
            return cachedResults;
        }

        await _semaphore.WaitAsync();
        try
        {
            var photos = await _client.SearchPhotosAsync(query, page: page, perPage: perPage);

            // Cache search results for shorter time
            _memoryCache.Set(cacheKey, photos, TimeSpan.FromMinutes(5));

            // Pre-cache individual photos
            foreach (var photo in photos)
            {
                var photoCacheKey = $"photo:{photo.Id}";
                _memoryCache.Set(photoCacheKey, photo, CalculateCacheDuration(photo));
            }

            return photos;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private TimeSpan CalculateCacheDuration(Photo photo)
    {
        // Popular photos cached longer
        var popularity = photo.Likes + (photo.Downloads / 10);

        return popularity switch
        {
            > 10000 => TimeSpan.FromHours(2),
            > 1000 => TimeSpan.FromHours(1),
            > 100 => TimeSpan.FromMinutes(30),
            _ => TimeSpan.FromMinutes(15)
        };
    }
}
```

### Batch Processing Optimization

```csharp
public class BatchPhotoProcessor
{
    private readonly UnsplasharpClient _client;
    private readonly ILogger<BatchPhotoProcessor> _logger;
    private readonly SemaphoreSlim _semaphore;

    public BatchPhotoProcessor(UnsplasharpClient client, ILogger<BatchPhotoProcessor> logger)
    {
        _client = client;
        _logger = logger;
        _semaphore = new SemaphoreSlim(3, 3); // Limit concurrent API calls
    }

    public async Task<List<Photo>> ProcessPhotosBatch(IEnumerable<string> photoIds, CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<Photo>();
        var batches = photoIds.Chunk(10); // Process in batches of 10

        foreach (var batch in batches)
        {
            var batchTasks = batch.Select(async photoId =>
            {
                await _semaphore.WaitAsync(cancellationToken);
                try
                {
                    var photo = await _client.GetPhotoAsync(photoId, cancellationToken);
                    results.Add(photo);

                    _logger.LogDebug("Processed photo {PhotoId}", photoId);
                }
                catch (UnsplasharpNotFoundException)
                {
                    _logger.LogWarning("Photo {PhotoId} not found", photoId);
                }
                catch (UnsplasharpRateLimitException ex)
                {
                    _logger.LogWarning("Rate limited, waiting {Delay}ms", ex.TimeUntilReset?.TotalMilliseconds);

                    if (ex.TimeUntilReset.HasValue)
                    {
                        await Task.Delay(ex.TimeUntilReset.Value, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing photo {PhotoId}", photoId);
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            await Task.WhenAll(batchTasks);

            // Rate limiting courtesy delay between batches
            await Task.Delay(500, cancellationToken);
        }

        return results.ToList();
    }
}
```

### Connection Pool Optimization

```csharp
public static class UnsplashHttpOptimization
{
    public static void ConfigureOptimizedHttpClient(this IServiceCollection services, string applicationId)
    {
        services.AddHttpClient("unsplash", client =>
        {
            client.BaseAddress = new Uri("https://api.unsplash.com/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", applicationId);
            client.Timeout = TimeSpan.FromSeconds(30);

            // Optimize headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // Connection pooling optimization
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 10,

            // Performance settings
            EnableMultipleHttp2Connections = true,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,

            // DNS optimization
            UseProxy = false
        });
    }
}
```

---

This comprehensive collection of code examples and recipes covers the most common use cases for Unsplasharp, from basic operations to advanced integration patterns. Each example includes proper error handling, performance considerations, and follows modern C# best practices.

Key takeaways:
- Always implement proper error handling with specific exception types
- Use caching strategies to improve performance and reduce API calls
- Implement rate limiting awareness in your applications
- Consider using dependency injection for better testability
- Use cancellation tokens for responsive applications
- Follow async/await best practices throughout your code
```
```
```
