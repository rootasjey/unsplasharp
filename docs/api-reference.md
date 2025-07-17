# API Reference Guide

This comprehensive guide covers all available methods in the Unsplasharp library with detailed examples and use cases.

## Table of Contents

- [Client Initialization](#client-initialization)
- [Photo Methods](#photo-methods)
- [Search Methods](#search-methods)
- [Collection Methods](#collection-methods)
- [User Methods](#user-methods)
- [Statistics Methods](#statistics-methods)
- [Method Parameters](#method-parameters)
- [Return Types](#return-types)

## Client Initialization

### Basic Initialization

```csharp
using Unsplasharp;

// Basic client with Application ID only
var client = new UnsplasharpClient("YOUR_APPLICATION_ID");

// With optional secret for authenticated requests
var client = new UnsplasharpClient("YOUR_APPLICATION_ID", "YOUR_SECRET");

// With logging support
var logger = loggerFactory.CreateLogger<UnsplasharpClient>();
var client = new UnsplasharpClient("YOUR_APPLICATION_ID", logger: logger);

// With IHttpClientFactory (recommended for production)
var client = new UnsplasharpClient("YOUR_APPLICATION_ID", 
    logger: logger, 
    httpClientFactory: httpClientFactory);
```

### Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
services.AddUnsplasharp("YOUR_APPLICATION_ID");

// Or with configuration
services.AddUnsplasharp(options =>
{
    options.ApplicationId = "YOUR_APPLICATION_ID";
    options.Secret = "YOUR_SECRET";
    options.ConfigureHttpClient = client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);
    };
});
```

## Photo Methods

### GetPhoto / GetPhotoAsync

Retrieve a specific photo by ID with optional custom sizing.

**Signatures:**
```csharp
Task<Photo?> GetPhoto(string id, CancellationToken cancellationToken = default)
Task<Photo?> GetPhoto(string id, int? width = null, int? height = null, CancellationToken cancellationToken = default)
Task<Photo?> GetPhoto(string id, int? width, int? height, int? cropX, int? cropY, int? cropWidth, int? cropHeight, CancellationToken cancellationToken = default)
Task<Photo> GetPhotoAsync(string id, CancellationToken cancellationToken = default) // Throws exceptions
```

**Examples:**

```csharp
// Basic photo retrieval
var photo = await client.GetPhoto("qcs09SwNPHY");
if (photo != null)
{
    Console.WriteLine($"Photo by {photo.User.Name}: {photo.Description}");
    Console.WriteLine($"Regular size: {photo.Urls.Regular}");
}

// Custom sizing
var resizedPhoto = await client.GetPhoto("qcs09SwNPHY", width: 800, height: 600);
Console.WriteLine($"Custom URL: {resizedPhoto?.Urls.Custom}");

// Cropped version
var croppedPhoto = await client.GetPhoto("qcs09SwNPHY", 
    width: 400, height: 400, 
    cropX: 100, cropY: 100, cropWidth: 200, cropHeight: 200);

// Exception-throwing version (recommended for new code)
try
{
    var photo = await client.GetPhotoAsync("qcs09SwNPHY");
    Console.WriteLine($"Photo dimensions: {photo.Width}x{photo.Height}");
}
catch (UnsplasharpNotFoundException)
{
    Console.WriteLine("Photo not found");
}
catch (UnsplasharpException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

### GetRandomPhoto / GetRandomPhotoAsync

Get one or more random photos with optional filtering.

**Signatures:**
```csharp
Task<Photo?> GetRandomPhoto(CancellationToken cancellationToken = default)
Task<Photo?> GetRandomPhoto(string collectionId, CancellationToken cancellationToken = default)
Task<Photo?> GetRandomPhoto(string[] collectionIds, CancellationToken cancellationToken = default)
Task<Photo?> GetRandomPhoto(int count, string? query = null, string? username = null, bool featured = false, string? collectionId = null, Orientation orientation = Orientation.All, CancellationToken cancellationToken = default)
Task<Photo> GetRandomPhotoAsync(...) // Exception-throwing versions
```

**Examples:**

```csharp
// Simple random photo
var randomPhoto = await client.GetRandomPhoto();

// Random photo from specific collection
var collectionPhoto = await client.GetRandomPhoto("499830");

// Random photo from multiple collections
var multiCollectionPhoto = await client.GetRandomPhoto(new[] { "499830", "194162" });

// Multiple random photos with filters
var naturePhotos = await client.GetRandomPhoto(
    count: 5,
    query: "nature",
    featured: true,
    orientation: Orientation.Landscape
);

// Random photo from specific user
var userPhoto = await client.GetRandomPhoto(1, username: "chrisjoelcampbell");
```

### ListPhotos / ListPhotosAsync

Get a paginated list of all photos.

**Signatures:**
```csharp
Task<List<Photo>> ListPhotos(int page = 1, int perPage = 10, OrderBy orderBy = OrderBy.Latest, CancellationToken cancellationToken = default)
Task<List<Photo>> ListPhotosAsync(...) // Exception-throwing version
```

**Examples:**

```csharp
// Get latest photos (default)
var latestPhotos = await client.ListPhotos(page: 1, perPage: 20);

// Get oldest photos first
var oldestPhotos = await client.ListPhotos(
    page: 1, 
    perPage: 10, 
    orderBy: OrderBy.Oldest
);

// Get popular photos
var popularPhotos = await client.ListPhotos(orderBy: OrderBy.Popular);

// Pagination example
for (int page = 1; page <= 5; page++)
{
    var photos = await client.ListPhotos(page: page, perPage: 30);
    Console.WriteLine($"Page {page}: {photos.Count} photos");
    
    foreach (var photo in photos)
    {
        Console.WriteLine($"  - {photo.Id}: {photo.Description}");
    }
}
```

## Search Methods

### SearchPhotos / SearchPhotosAsync

Search for photos by query with advanced filtering options.

**Signatures:**
```csharp
Task<List<Photo>> SearchPhotos(string query, int page = 1, int perPage = 10, CancellationToken cancellationToken = default)
Task<List<Photo>> SearchPhotos(string query, int page, int perPage, OrderBy orderBy, string? collectionIds = null, string? contentFilter = null, string? color = null, Orientation orientation = Orientation.All, CancellationToken cancellationToken = default)
```

**Examples:**

```csharp
// Basic search
var naturePhotos = await client.SearchPhotos("nature");

// Advanced search with filters
var filteredPhotos = await client.SearchPhotos(
    query: "mountain landscape",
    page: 1,
    perPage: 20,
    orderBy: OrderBy.Relevant,
    color: "blue",
    orientation: Orientation.Landscape
);

// Search with content filtering
var safePhotos = await client.SearchPhotos(
    query: "beach",
    page: 1,
    perPage: 15,
    contentFilter: "high"
);

// Check search metadata
Console.WriteLine($"Search query: {client.LastPhotosSearchQuery}");
Console.WriteLine($"Total results: {client.LastPhotosSearchTotalResults}");
Console.WriteLine($"Total pages: {client.LastPhotosSearchTotalPages}");
```

## Collection Methods

### GetCollection / GetCollectionAsync

Retrieve a specific collection by ID.

**Signatures:**
```csharp
Task<Collection?> GetCollection(string id, CancellationToken cancellationToken = default)
Task<Collection> GetCollectionAsync(string id, CancellationToken cancellationToken = default)
```

**Examples:**

```csharp
// Get collection details
var collection = await client.GetCollection("499830");
if (collection != null)
{
    Console.WriteLine($"Collection: {collection.Title}");
    Console.WriteLine($"Description: {collection.Description}");
    Console.WriteLine($"Total photos: {collection.TotalPhotos}");
    Console.WriteLine($"Created by: {collection.User.Name}");
}

// Exception-throwing version
try
{
    var collection = await client.GetCollectionAsync("499830");
    Console.WriteLine($"Collection cover: {collection.CoverPhoto.Urls.Regular}");
}
catch (UnsplasharpNotFoundException)
{
    Console.WriteLine("Collection not found");
}
```

### ListCollections / ListCollectionsAsync

Get a paginated list of all collections.

**Signatures:**
```csharp
Task<List<Collection>> ListCollections(int page = 1, int perPage = 10, CancellationToken cancellationToken = default)
Task<List<Collection>> ListCollectionsAsync(...) // Exception-throwing version
```

**Examples:**

```csharp
// Get featured collections
var collections = await client.ListCollections(page: 1, perPage: 20);

foreach (var collection in collections)
{
    Console.WriteLine($"{collection.Title} - {collection.TotalPhotos} photos");
    Console.WriteLine($"  By: {collection.User.Name}");
    Console.WriteLine($"  Cover: {collection.CoverPhoto.Urls.Small}");
}

// Pagination through collections
var allCollections = new List<Collection>();
int page = 1;
List<Collection> pageResults;

do
{
    pageResults = await client.ListCollections(page: page, perPage: 30);
    allCollections.AddRange(pageResults);
    page++;
} while (pageResults.Count == 30); // Continue while getting full pages
```

### GetCollectionPhotos / GetCollectionPhotosAsync

Get photos from a specific collection.

**Signatures:**
```csharp
Task<List<Photo>> GetCollectionPhotos(string collectionId, int page = 1, int perPage = 10, CancellationToken cancellationToken = default)
Task<List<Photo>> GetCollectionPhotosAsync(...) // Exception-throwing version
```

**Examples:**

```csharp
// Get photos from a collection
var collectionPhotos = await client.GetCollectionPhotos("499830", page: 1, perPage: 25);

Console.WriteLine($"Found {collectionPhotos.Count} photos in collection");
foreach (var photo in collectionPhotos)
{
    Console.WriteLine($"  {photo.Id}: {photo.Description ?? "No description"}");
    Console.WriteLine($"    By: {photo.User.Name}");
    Console.WriteLine($"    Likes: {photo.Likes}, Downloads: {photo.Downloads}");
}
```

### SearchCollections / SearchCollectionsAsync

Search for collections by query.

**Signatures:**
```csharp
Task<List<Collection>> SearchCollections(string query, int page = 1, int perPage = 10, CancellationToken cancellationToken = default)
Task<List<Collection>> SearchCollectionsAsync(...) // Exception-throwing version
```

**Examples:**

```csharp
// Search for collections
var travelCollections = await client.SearchCollections("travel", page: 1, perPage: 15);

Console.WriteLine($"Found {travelCollections.Count} travel collections");
foreach (var collection in travelCollections)
{
    Console.WriteLine($"{collection.Title} ({collection.TotalPhotos} photos)");
    Console.WriteLine($"  {collection.Description}");
}

// Check search metadata
Console.WriteLine($"Search query: {client.LastCollectionsSearchQuery}");
Console.WriteLine($"Total results: {client.LastCollectionsSearchTotalResults}");
Console.WriteLine($"Total pages: {client.LastCollectionsSearchTotalPages}");
```

## User Methods

### GetUser / GetUserAsync

Retrieve a user's profile information.

**Signatures:**
```csharp
Task<User?> GetUser(string username, CancellationToken cancellationToken = default)
Task<User> GetUserAsync(string username, CancellationToken cancellationToken = default)
```

**Examples:**

```csharp
// Get user profile
var user = await client.GetUser("chrisjoelcampbell");
if (user != null)
{
    Console.WriteLine($"Name: {user.Name}");
    Console.WriteLine($"Username: {user.Username}");
    Console.WriteLine($"Bio: {user.Bio}");
    Console.WriteLine($"Location: {user.Location}");
    Console.WriteLine($"Portfolio: {user.PortfolioUrl}");
    Console.WriteLine($"Total photos: {user.TotalPhotos}");
    Console.WriteLine($"Total likes: {user.TotalLikes}");
    Console.WriteLine($"Total collections: {user.TotalCollections}");
    Console.WriteLine($"Profile image: {user.ProfileImage.Large}");
}

// Exception-throwing version
try
{
    var user = await client.GetUserAsync("nonexistentuser");
    Console.WriteLine($"User found: {user.Name}");
}
catch (UnsplasharpNotFoundException)
{
    Console.WriteLine("User not found");
}
```

### GetUserPhotos / GetUserPhotosAsync

Get photos uploaded by a specific user.

**Signatures:**
```csharp
Task<List<Photo>> GetUserPhotos(string username, int page = 1, int perPage = 10, OrderBy orderBy = OrderBy.Latest, bool stats = false, Resolution resolution = Resolution.All, int quantity = 30, Orientation orientation = Orientation.All, CancellationToken cancellationToken = default)
```

**Examples:**

```csharp
// Get user's photos
var userPhotos = await client.GetUserPhotos("chrisjoelcampbell", page: 1, perPage: 20);

Console.WriteLine($"Photos by user: {userPhotos.Count}");
foreach (var photo in userPhotos)
{
    Console.WriteLine($"  {photo.Id}: {photo.Description ?? "Untitled"}");
    Console.WriteLine($"    Dimensions: {photo.Width}x{photo.Height}");
    Console.WriteLine($"    Likes: {photo.Likes}");
}

// Get user's popular photos
var popularPhotos = await client.GetUserPhotos(
    username: "chrisjoelcampbell",
    orderBy: OrderBy.Popular,
    perPage: 10
);

// Get user's landscape photos only
var landscapePhotos = await client.GetUserPhotos(
    username: "chrisjoelcampbell",
    orientation: Orientation.Landscape,
    perPage: 15
);
```

### GetUserLikes / GetUserLikesAsync

Get photos liked by a specific user.

**Signatures:**
```csharp
Task<List<Photo>> GetUserLikes(string username, int page = 1, int perPage = 10, OrderBy orderBy = OrderBy.Latest, Orientation orientation = Orientation.All, CancellationToken cancellationToken = default)
```

**Examples:**

```csharp
// Get user's liked photos
var likedPhotos = await client.GetUserLikes("chrisjoelcampbell", page: 1, perPage: 20);

Console.WriteLine($"Photos liked by user: {likedPhotos.Count}");
foreach (var photo in likedPhotos)
{
    Console.WriteLine($"  {photo.Id} by {photo.User.Name}");
    Console.WriteLine($"    {photo.Description ?? "No description"}");
}
```

### GetUserCollections / GetUserCollectionsAsync

Get collections created by a specific user.

**Signatures:**
```csharp
Task<List<Collection>> GetUserCollections(string username, int page = 1, int perPage = 10, CancellationToken cancellationToken = default)
```

**Examples:**

```csharp
// Get user's collections
var userCollections = await client.GetUserCollections("chrisjoelcampbell");

Console.WriteLine($"Collections by user: {userCollections.Count}");
foreach (var collection in userCollections)
{
    Console.WriteLine($"  {collection.Title} ({collection.TotalPhotos} photos)");
    Console.WriteLine($"    {collection.Description}");
}
```

## Statistics Methods

### GetTotalStats / GetTotalStatsAsync

Get overall Unsplash statistics.

**Signatures:**
```csharp
Task<UnplashTotalStats?> GetTotalStats(CancellationToken cancellationToken = default)
Task<UnplashTotalStats> GetTotalStatsAsync(CancellationToken cancellationToken = default)
```

**Examples:**

```csharp
// Get total platform statistics
var totalStats = await client.GetTotalStats();
if (totalStats != null)
{
    Console.WriteLine($"Total photos: {totalStats.Photos:N0}");
    Console.WriteLine($"Total downloads: {totalStats.Downloads:N0}");
    Console.WriteLine($"Total views: {totalStats.Views:N0}");
    Console.WriteLine($"Total likes: {totalStats.Likes:N0}");
    Console.WriteLine($"Total photographers: {totalStats.Photographers:N0}");
    Console.WriteLine($"Total pixels served: {totalStats.PixelsServed:N0}");
    Console.WriteLine($"Total views this month: {totalStats.ViewsThisMonth:N0}");
    Console.WriteLine($"Total new photos this month: {totalStats.NewPhotosThisMonth:N0}");
}
```

### GetMonthlyStats / GetMonthlyStatsAsync

Get Unsplash statistics for the past 30 days.

**Signatures:**
```csharp
Task<UnplashMonthlyStats?> GetMonthlyStats(CancellationToken cancellationToken = default)
Task<UnplashMonthlyStats> GetMonthlyStatsAsync(CancellationToken cancellationToken = default)
```

**Examples:**

```csharp
// Get monthly statistics
var monthlyStats = await client.GetMonthlyStats();
if (monthlyStats != null)
{
    Console.WriteLine($"Downloads this month: {monthlyStats.Downloads:N0}");
    Console.WriteLine($"Views this month: {monthlyStats.Views:N0}");
    Console.WriteLine($"Likes this month: {monthlyStats.Likes:N0}");
    Console.WriteLine($"New photos this month: {monthlyStats.NewPhotos:N0}");
    Console.WriteLine($"New photographers this month: {monthlyStats.NewPhotographers:N0}");
    Console.WriteLine($"New pixels this month: {monthlyStats.NewPixels:N0}");
    Console.WriteLine($"New developers this month: {monthlyStats.NewDevelopers:N0}");
    Console.WriteLine($"New applications this month: {monthlyStats.NewApplications:N0}");
    Console.WriteLine($"New requests this month: {monthlyStats.NewRequests:N0}");
}
```

## Method Parameters

### Common Parameters

#### Pagination Parameters
- **`page`** (int): Page number to retrieve (1-based indexing). Default: 1
- **`perPage`** (int): Number of items per page. Default: 10, Maximum: 30

#### Ordering Parameters
- **`orderBy`** (OrderBy enum):
  - `OrderBy.Latest` - Most recent first (default)
  - `OrderBy.Oldest` - Oldest first
  - `OrderBy.Popular` - Most popular first
  - `OrderBy.Relevant` - Most relevant (for search queries)

#### Orientation Parameters
- **`orientation`** (Orientation enum):
  - `Orientation.All` - All orientations (default)
  - `Orientation.Landscape` - Landscape photos only
  - `Orientation.Portrait` - Portrait photos only
  - `Orientation.Squarish` - Square-ish photos only

#### Resolution Parameters
- **`resolution`** (Resolution enum):
  - `Resolution.All` - All resolutions (default)
  - `Resolution.Regular` - Regular resolution
  - `Resolution.Small` - Small resolution

### Photo-Specific Parameters

#### Custom Sizing Parameters
- **`width`** (int?): Custom width in pixels
- **`height`** (int?): Custom height in pixels
- **`cropX`** (int?): X coordinate for cropping
- **`cropY`** (int?): Y coordinate for cropping
- **`cropWidth`** (int?): Width of crop area
- **`cropHeight`** (int?): Height of crop area

#### Search Filters
- **`query`** (string): Search query string
- **`color`** (string?): Filter by color ("black_and_white", "black", "white", "yellow", "orange", "red", "purple", "magenta", "green", "teal", "blue")
- **`contentFilter`** (string?): Content safety filter ("low", "high")
- **`collectionIds`** (string?): Comma-separated collection IDs to search within

#### Random Photo Parameters
- **`count`** (int): Number of random photos to return (1-30)
- **`featured`** (bool): Only featured photos
- **`username`** (string?): Photos from specific user only
- **`collectionId`** (string?): Photos from specific collection only
- **`collectionIds`** (string[]): Photos from multiple collections

## Return Types

### Photo Model
The `Photo` class contains comprehensive information about an Unsplash photo:

```csharp
public class Photo
{
    public string Id { get; set; }                    // Unique photo identifier
    public string Description { get; set; }           // Photo description
    public string CreatedAt { get; set; }            // Creation timestamp
    public string UpdatedAt { get; set; }            // Last update timestamp
    public int Width { get; set; }                   // Photo width in pixels
    public int Height { get; set; }                 // Photo height in pixels
    public string Color { get; set; }               // Dominant color
    public string BlurHash { get; set; }            // BlurHash placeholder
    public int Downloads { get; set; }              // Download count
    public int Likes { get; set; }                  // Like count
    public bool IsLikedByUser { get; set; }         // User like status

    // Complex properties
    public Urls Urls { get; set; }                  // Photo URLs
    public User User { get; set; }                  // Photo author
    public Exif Exif { get; set; }                 // Camera EXIF data
    public Location Location { get; set; }          // Photo location
    public PhotoLinks Links { get; set; }           // Related links
    public List<Category> Categories { get; set; }  // Photo categories
    public List<Collection> CurrentUserCollection { get; set; } // User collections
}
```

### Collection Model
The `Collection` class represents a curated collection of photos:

```csharp
public class Collection
{
    public string Id { get; set; }                  // Unique collection identifier
    public string Title { get; set; }              // Collection title
    public string Description { get; set; }        // Collection description
    public string PublishedAt { get; set; }        // Publication timestamp
    public string UpdatedAt { get; set; }          // Last update timestamp
    public int TotalPhotos { get; set; }           // Number of photos
    public bool IsPrivate { get; set; }            // Privacy status
    public string ShareKey { get; set; }           // Share key for private collections

    // Complex properties
    public Photo CoverPhoto { get; set; }          // Collection cover photo
    public User User { get; set; }                // Collection creator
    public CollectionLinks Links { get; set; }     // Related links
}
```

### User Model
The `User` class contains user profile information:

```csharp
public class User
{
    public string Id { get; set; }                 // Unique user identifier
    public string Username { get; set; }           // Username
    public string Name { get; set; }               // Display name
    public string FirstName { get; set; }          // First name
    public string LastName { get; set; }           // Last name
    public string TwitterUsername { get; set; }    // Twitter handle
    public string PortfolioUrl { get; set; }       // Portfolio URL
    public string Bio { get; set; }                // User biography
    public string Location { get; set; }           // User location
    public int TotalLikes { get; set; }            // Total likes received
    public int TotalPhotos { get; set; }           // Total photos uploaded
    public int TotalCollections { get; set; }      // Total collections created
    public bool ForHire { get; set; }              // Available for hire

    // Complex properties
    public ProfileImage ProfileImage { get; set; } // Profile image URLs
    public Badge Badge { get; set; }               // User badge info
    public UserLinks Links { get; set; }           // Related links
}
```

### URL Types
Different photo sizes are available through the `Urls` class:

```csharp
public class Urls
{
    public string Raw { get; set; }        // Full resolution, uncompressed
    public string Full { get; set; }       // Large size (max 2048px)
    public string Regular { get; set; }    // Medium size (max 1080px)
    public string Small { get; set; }      // Small size (max 400px)
    public string Thumbnail { get; set; }  // Thumbnail (max 200px)
    public string Custom { get; set; }     // Custom size (when specified)
}
```

## Rate Limiting

### Rate Limit Information
The client automatically tracks rate limit information:

```csharp
var client = new UnsplasharpClient("YOUR_APP_ID");

// Make a request
var photo = await client.GetRandomPhoto();

// Check rate limit status
Console.WriteLine($"Rate limit: {client.RateLimitRemaining}/{client.MaxRateLimit}");
```

### Rate Limit Headers
Unsplash provides rate limit information in response headers:
- `X-Ratelimit-Limit`: Maximum requests per hour
- `X-Ratelimit-Remaining`: Remaining requests in current hour

### Handling Rate Limits
Use the exception-throwing methods for better rate limit handling:

```csharp
try
{
    var photos = await client.SearchPhotosAsync("nature");
}
catch (UnsplasharpRateLimitException ex)
{
    Console.WriteLine($"Rate limit exceeded: {ex.RateLimitRemaining}/{ex.RateLimit}");
    Console.WriteLine($"Reset time: {ex.RateLimitReset}");

    // Wait until reset or implement exponential backoff
    if (ex.RateLimitReset.HasValue)
    {
        var waitTime = ex.RateLimitReset.Value - DateTimeOffset.UtcNow;
        await Task.Delay(waitTime);
    }
}
```

## Best Practices

### 1. Use Exception-Throwing Methods
For new code, prefer the `*Async` methods that throw exceptions:

```csharp
// Preferred approach
try
{
    var photo = await client.GetPhotoAsync("photo-id");
    // Handle success
}
catch (UnsplasharpNotFoundException)
{
    // Handle not found
}
catch (UnsplasharpException ex)
{
    // Handle other errors
}
```

### 2. Implement Proper Error Handling
Always handle potential exceptions appropriately:

```csharp
public async Task<Photo?> GetPhotoSafely(string photoId)
{
    try
    {
        return await client.GetPhotoAsync(photoId);
    }
    catch (UnsplasharpNotFoundException)
    {
        logger.LogWarning("Photo {PhotoId} not found", photoId);
        return null;
    }
    catch (UnsplasharpRateLimitException ex)
    {
        logger.LogWarning("Rate limit exceeded, retry after {ResetTime}", ex.RateLimitReset);
        throw; // Re-throw to handle at higher level
    }
    catch (UnsplasharpException ex)
    {
        logger.LogError(ex, "Unsplash API error for photo {PhotoId}", photoId);
        throw;
    }
}
```

### 3. Use Cancellation Tokens
Always pass cancellation tokens for better responsiveness:

```csharp
public async Task<List<Photo>> SearchWithTimeout(string query, TimeSpan timeout)
{
    using var cts = new CancellationTokenSource(timeout);

    try
    {
        return await client.SearchPhotosAsync(query, cancellationToken: cts.Token);
    }
    catch (OperationCanceledException)
    {
        logger.LogWarning("Search for '{Query}' timed out after {Timeout}", query, timeout);
        return new List<Photo>();
    }
}
```

### 4. Respect Rate Limits
Monitor and respect API rate limits:

```csharp
public async Task<List<Photo>> GetPhotosWithRateLimit(List<string> photoIds)
{
    var photos = new List<Photo>();

    foreach (var photoId in photoIds)
    {
        // Check rate limit before making request
        if (client.RateLimitRemaining < 10)
        {
            logger.LogWarning("Rate limit running low, pausing requests");
            await Task.Delay(TimeSpan.FromMinutes(1));
        }

        try
        {
            var photo = await client.GetPhotoAsync(photoId);
            photos.Add(photo);
        }
        catch (UnsplasharpRateLimitException)
        {
            logger.LogWarning("Rate limit exceeded, stopping batch operation");
            break;
        }
    }

    return photos;
}
```
