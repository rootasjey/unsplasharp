# Unsplasharp Quick Reference

Fast reference for common operations and patterns. Perfect for developers who need quick access to code snippets and method signatures.

## 🚀 Quick Setup

```csharp
// Basic setup
var client = new UnsplasharpClient("YOUR_APPLICATION_ID");

// With dependency injection
services.AddUnsplasharp("YOUR_APPLICATION_ID");

// With logging
var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var client = new UnsplasharpClient("YOUR_APPLICATION_ID", logger: logger);
```

## 📸 Common Operations

### Get Random Photo
```csharp
// Basic
var photo = await client.GetRandomPhotoAsync();

// With query
var photo = await client.GetRandomPhotoAsync(query: "nature");

// With filters
var photo = await client.GetRandomPhotoAsync(
    query: "landscape", 
    orientation: Orientation.Landscape
);
```

### Search Photos
```csharp
// Basic search
var photos = await client.SearchPhotosAsync("mountain");

// Advanced search
var photos = await client.SearchPhotosAsync(
    query: "sunset beach",
    page: 1,
    perPage: 20,
    orderBy: OrderBy.Popular,
    color: "orange",
    orientation: Orientation.Landscape
);
```

### Get Specific Photo
```csharp
// By ID
var photo = await client.GetPhotoAsync("photo-id");

// With custom size
var photo = await client.GetPhoto("photo-id", width: 800, height: 600);
```

### Get User Information
```csharp
// User profile
var user = await client.GetUserAsync("username");

// User's photos
var photos = await client.GetUserPhotosAsync("username", perPage: 20);

// User's likes
var likes = await client.GetUserLikesAsync("username");
```

### Collections
```csharp
// Get collection
var collection = await client.GetCollectionAsync("collection-id");

// Collection photos
var photos = await client.GetCollectionPhotosAsync("collection-id");

// Search collections
var collections = await client.SearchCollectionsAsync("travel");
```

## ⚠️ Error Handling

### Basic Pattern
```csharp
try
{
    var photo = await client.GetPhotoAsync("photo-id");
    // Success
}
catch (UnsplasharpNotFoundException)
{
    // Photo not found
}
catch (UnsplasharpRateLimitException ex)
{
    // Rate limited - wait until ex.RateLimitReset
}
catch (UnsplasharpException ex)
{
    // Other API errors
}
```

### Exception Types
- `UnsplasharpNotFoundException` - Resource not found (404)
- `UnsplasharpRateLimitException` - Rate limit exceeded (429)
- `UnsplasharpAuthenticationException` - Invalid API key (401)
- `UnsplasharpNetworkException` - Network/connection issues
- `UnsplasharpTimeoutException` - Request timeout
- `UnsplasharpException` - Base exception type

## 🔄 Rate Limiting

### Check Rate Limit
```csharp
Console.WriteLine($"Rate limit: {client.RateLimitRemaining}/{client.MaxRateLimit}");
```

### Handle Rate Limits
```csharp
try
{
    var photos = await client.SearchPhotosAsync("nature");
}
catch (UnsplasharpRateLimitException ex)
{
    var waitTime = ex.TimeUntilReset ?? TimeSpan.FromMinutes(1);
    await Task.Delay(waitTime);
    // Retry request
}
```

## 📊 Data Models

### Photo Properties
```csharp
photo.Id              // Unique identifier
photo.Description     // Photo description
photo.Width           // Width in pixels
photo.Height          // Height in pixels
photo.Color           // Dominant color (hex)
photo.Likes           // Like count
photo.Downloads       // Download count
photo.User            // Photographer info
photo.Urls            // Different sizes
photo.Exif            // Camera data
photo.Location        // GPS location
```

### Photo URLs
```csharp
photo.Urls.Thumbnail  // ~200px
photo.Urls.Small      // ~400px
photo.Urls.Regular    // ~1080px
photo.Urls.Full       // ~2048px
photo.Urls.Raw        // Original size
```

### User Properties
```csharp
user.Id               // Unique identifier
user.Username         // Username
user.Name             // Display name
user.Bio              // Biography
user.Location         // Location
user.TotalPhotos      // Photo count
user.TotalLikes       // Likes received
user.ProfileImage     // Avatar URLs
```

## 🔍 Search Parameters

### Common Parameters
```csharp
query: "search term"           // Search query
page: 1                        // Page number (1-based)
perPage: 20                    // Results per page (max 30)
orderBy: OrderBy.Popular       // Latest, Oldest, Popular, Relevant
```

### Photo Search Filters
```csharp
color: "blue"                  // Color filter
orientation: Orientation.Landscape  // Landscape, Portrait, Squarish
contentFilter: "high"          // Content safety (low, high)
```

### Orientation Options
- `Orientation.All` - All orientations
- `Orientation.Landscape` - Landscape only
- `Orientation.Portrait` - Portrait only
- `Orientation.Squarish` - Square-ish only

### Order Options
- `OrderBy.Latest` - Most recent first
- `OrderBy.Oldest` - Oldest first
- `OrderBy.Popular` - Most popular first
- `OrderBy.Relevant` - Most relevant (search only)

## 💾 Caching Example

```csharp
public class CachedPhotoService
{
    private readonly UnsplasharpClient _client;
    private readonly IMemoryCache _cache;

    public async Task<Photo?> GetPhotoAsync(string photoId)
    {
        var cacheKey = $"photo:{photoId}";
        
        if (_cache.TryGetValue(cacheKey, out Photo cachedPhoto))
            return cachedPhoto;

        try
        {
            var photo = await _client.GetPhotoAsync(photoId);
            _cache.Set(cacheKey, photo, TimeSpan.FromHours(1));
            return photo;
        }
        catch (UnsplasharpNotFoundException)
        {
            _cache.Set(cacheKey, (Photo?)null, TimeSpan.FromMinutes(5));
            return null;
        }
    }
}
```

## 🔧 Configuration

### ASP.NET Core Setup
```csharp
// Program.cs
services.AddUnsplasharp(options =>
{
    options.ApplicationId = Configuration["Unsplash:ApplicationId"];
    options.ConfigureHttpClient = client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    };
});
```

### appsettings.json
```json
{
  "Unsplash": {
    "ApplicationId": "your-app-id-here"
  }
}
```

## 📥 Download Images

```csharp
public async Task DownloadPhoto(Photo photo, string filePath)
{
    using var httpClient = new HttpClient();
    var imageBytes = await httpClient.GetByteArrayAsync(photo.Urls.Regular);
    await File.WriteAllBytesAsync(filePath, imageBytes);
}
```

## 🔄 Pagination

```csharp
public async Task<List<Photo>> GetAllPhotos(string query, int maxPhotos = 100)
{
    var allPhotos = new List<Photo>();
    var page = 1;
    var perPage = 30;

    while (allPhotos.Count < maxPhotos)
    {
        var photos = await client.SearchPhotosAsync(query, page: page, perPage: perPage);
        
        if (photos.Count == 0) break;
        
        allPhotos.AddRange(photos);
        page++;
        
        // Rate limiting courtesy delay
        await Task.Delay(100);
    }

    return allPhotos.Take(maxPhotos).ToList();
}
```

## 🧪 Testing

### Unit Test Example
```csharp
[Test]
public async Task GetPhoto_WithValidId_ReturnsPhoto()
{
    // Arrange
    var mockClient = new Mock<UnsplasharpClient>("test-app-id");
    var expectedPhoto = new Photo { Id = "test-id" };
    mockClient.Setup(c => c.GetPhotoAsync("test-id", It.IsAny<CancellationToken>()))
              .ReturnsAsync(expectedPhoto);

    // Act
    var result = await mockClient.Object.GetPhotoAsync("test-id");

    // Assert
    Assert.AreEqual("test-id", result.Id);
}
```

## 🚨 Common Pitfalls

### ❌ Don't Do This
```csharp
// Don't ignore rate limits
for (int i = 0; i < 100; i++)
{
    await client.GetRandomPhotoAsync(); // Will hit rate limit
}

// Don't create multiple clients
var client1 = new UnsplasharpClient("app-id");
var client2 = new UnsplasharpClient("app-id"); // Wasteful

// Don't hardcode API keys
var client = new UnsplasharpClient("hardcoded-key"); // Security risk
```

### ✅ Do This Instead
```csharp
// Handle rate limits
try
{
    await client.GetRandomPhotoAsync();
}
catch (UnsplasharpRateLimitException ex)
{
    await Task.Delay(ex.TimeUntilReset ?? TimeSpan.FromMinutes(1));
}

// Use dependency injection
services.AddUnsplasharp("app-id");

// Use configuration
var appId = Configuration["Unsplash:ApplicationId"];
var client = new UnsplasharpClient(appId);
```

## 📱 Method Signatures Quick Reference

```csharp
// Photos
Task<Photo> GetPhotoAsync(string id, CancellationToken cancellationToken = default)
Task<Photo> GetRandomPhotoAsync(CancellationToken cancellationToken = default)
Task<List<Photo>> SearchPhotosAsync(string query, int page = 1, int perPage = 10, ...)
Task<List<Photo>> ListPhotosAsync(int page = 1, int perPage = 10, ...)

// Users
Task<User> GetUserAsync(string username, CancellationToken cancellationToken = default)
Task<List<Photo>> GetUserPhotosAsync(string username, int page = 1, int perPage = 10, ...)
Task<List<Photo>> GetUserLikesAsync(string username, int page = 1, int perPage = 10, ...)

// Collections
Task<Collection> GetCollectionAsync(string id, CancellationToken cancellationToken = default)
Task<List<Photo>> GetCollectionPhotosAsync(string collectionId, int page = 1, int perPage = 10, ...)
Task<List<Collection>> SearchCollectionsAsync(string query, int page = 1, int perPage = 10, ...)

// Statistics
Task<UnplashTotalStats> GetTotalStatsAsync(CancellationToken cancellationToken = default)
Task<UnplashMonthlyStats> GetMonthlyStatsAsync(CancellationToken cancellationToken = default)
```

---

**Need more details?** Check the [complete documentation](index.md) or [API reference](api-reference.md).
