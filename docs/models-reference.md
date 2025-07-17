# Models Reference Guide

This comprehensive guide covers all model classes in Unsplasharp, their properties, relationships, and usage examples.

## Table of Contents

- [Photo Model](#photo-model)
- [User Model](#user-model)
- [Collection Model](#collection-model)
- [URL Models](#url-models)
- [Location and EXIF Models](#location-and-exif-models)
- [Statistics Models](#statistics-models)
- [Link Models](#link-models)
- [Supporting Models](#supporting-models)
- [Model Relationships](#model-relationships)
- [Usage Examples](#usage-examples)

## Photo Model

The `Photo` class is the core model representing an Unsplash photo with comprehensive metadata.

### Properties

```csharp
public class Photo : INotifyPropertyChanged
{
    // Basic Properties
    public string Id { get; set; }                    // Unique photo identifier
    public string Description { get; set; }           // Photo description/alt text
    public string CreatedAt { get; set; }            // ISO 8601 creation timestamp
    public string UpdatedAt { get; set; }            // ISO 8601 last update timestamp
    public int Width { get; set; }                   // Photo width in pixels
    public int Height { get; set; }                 // Photo height in pixels
    public string Color { get; set; }               // Dominant color (hex format)
    public string BlurHash { get; set; }            // BlurHash for placeholder
    
    // Engagement Metrics
    public int Downloads { get; set; }              // Total download count
    public int Likes { get; set; }                  // Total like count
    public bool IsLikedByUser { get; set; }         // Current user's like status
    
    // Complex Properties
    public Urls Urls { get; set; }                  // Photo URLs in different sizes
    public User User { get; set; }                  // Photo author/photographer
    public Exif Exif { get; set; }                 // Camera EXIF data
    public Location Location { get; set; }          // Photo location data
    public PhotoLinks Links { get; set; }           // Related API links
    public List<Category> Categories { get; set; }  // Photo categories/tags
    public List<Collection> CurrentUserCollection { get; set; } // User's collections
}
```

### Usage Examples

```csharp
// Basic photo information
var photo = await client.GetPhotoAsync("qcs09SwNPHY");

Console.WriteLine($"Photo ID: {photo.Id}");
Console.WriteLine($"Title: {photo.Description ?? "Untitled"}");
Console.WriteLine($"Photographer: {photo.User.Name} (@{photo.User.Username})");
Console.WriteLine($"Dimensions: {photo.Width}x{photo.Height}");
Console.WriteLine($"Aspect Ratio: {(double)photo.Width / photo.Height:F2}");
Console.WriteLine($"Dominant Color: {photo.Color}");
Console.WriteLine($"Engagement: {photo.Likes:N0} likes, {photo.Downloads:N0} downloads");

// Check if photo has location data
if (!string.IsNullOrEmpty(photo.Location.Name))
{
    Console.WriteLine($"Location: {photo.Location.Name}");
    if (photo.Location.Position != null)
    {
        Console.WriteLine($"Coordinates: {photo.Location.Position.Latitude}, {photo.Location.Position.Longitude}");
    }
}

// Check camera information
if (!string.IsNullOrEmpty(photo.Exif.Make))
{
    Console.WriteLine($"Camera: {photo.Exif.Make} {photo.Exif.Model}");
    Console.WriteLine($"Settings: f/{photo.Exif.Aperture}, {photo.Exif.ExposureTime}s, ISO {photo.Exif.Iso}");
}

// Access different photo sizes
Console.WriteLine("Available sizes:");
Console.WriteLine($"  Thumbnail: {photo.Urls.Thumbnail}");
Console.WriteLine($"  Small: {photo.Urls.Small}");
Console.WriteLine($"  Regular: {photo.Urls.Regular}");
Console.WriteLine($"  Full: {photo.Urls.Full}");
Console.WriteLine($"  Raw: {photo.Urls.Raw}");
```

### Photo Filtering and Analysis

```csharp
public static class PhotoAnalyzer
{
    public static bool IsLandscape(Photo photo) => photo.Width > photo.Height;
    public static bool IsPortrait(Photo photo) => photo.Height > photo.Width;
    public static bool IsSquare(Photo photo) => Math.Abs(photo.Width - photo.Height) < 50;
    
    public static bool IsHighResolution(Photo photo) => photo.Width >= 1920 && photo.Height >= 1080;
    public static bool IsPopular(Photo photo) => photo.Likes > 1000 || photo.Downloads > 10000;
    
    public static string GetOrientationDescription(Photo photo)
    {
        return photo.Width switch
        {
            var w when w > photo.Height * 1.5 => "Wide Landscape",
            var w when w > photo.Height => "Landscape",
            var w when w < photo.Height / 1.5 => "Tall Portrait",
            var w when w < photo.Height => "Portrait",
            _ => "Square"
        };
    }
    
    public static PhotoQuality AssessQuality(Photo photo)
    {
        var score = 0;
        
        // Resolution scoring
        if (photo.Width >= 3840 && photo.Height >= 2160) score += 3; // 4K+
        else if (photo.Width >= 1920 && photo.Height >= 1080) score += 2; // Full HD+
        else if (photo.Width >= 1280 && photo.Height >= 720) score += 1; // HD+
        
        // Engagement scoring
        if (photo.Likes > 10000) score += 3;
        else if (photo.Likes > 1000) score += 2;
        else if (photo.Likes > 100) score += 1;
        
        // EXIF data availability
        if (!string.IsNullOrEmpty(photo.Exif.Make)) score += 1;
        
        return score switch
        {
            >= 6 => PhotoQuality.Excellent,
            >= 4 => PhotoQuality.Good,
            >= 2 => PhotoQuality.Average,
            _ => PhotoQuality.Basic
        };
    }
}

public enum PhotoQuality
{
    Basic,
    Average,
    Good,
    Excellent
}
```

## User Model

The `User` class represents an Unsplash photographer or user profile.

### Properties

```csharp
public class User : INotifyPropertyChanged
{
    // Identity
    public string Id { get; set; }                 // Unique user identifier
    public string Username { get; set; }           // Username (handle)
    public string Name { get; set; }               // Display name
    public string FirstName { get; set; }          // First name
    public string LastName { get; set; }           // Last name
    
    // Profile Information
    public string Bio { get; set; }                // User biography
    public string Location { get; set; }           // User location
    public string PortfolioUrl { get; set; }       // Portfolio website
    public string TwitterUsername { get; set; }    // Twitter handle
    public bool ForHire { get; set; }              // Available for hire
    
    // Statistics
    public int TotalLikes { get; set; }            // Total likes received
    public int TotalPhotos { get; set; }           // Total photos uploaded
    public int TotalCollections { get; set; }      // Total collections created
    public int FollowersCount { get; set; }        // Number of followers
    public int FollowingCount { get; set; }        // Number of following
    
    // Complex Properties
    public ProfileImage ProfileImage { get; set; } // Profile image URLs
    public Badge Badge { get; set; }               // User badge information
    public UserLinks Links { get; set; }           // Related API links
}
```

### Usage Examples

```csharp
// Get user profile information
var user = await client.GetUserAsync("chrisjoelcampbell");

Console.WriteLine($"Photographer: {user.Name} (@{user.Username})");
Console.WriteLine($"Bio: {user.Bio}");
Console.WriteLine($"Location: {user.Location}");
Console.WriteLine($"Portfolio: {user.PortfolioUrl}");

// Statistics
Console.WriteLine($"Statistics:");
Console.WriteLine($"  Photos: {user.TotalPhotos:N0}");
Console.WriteLine($"  Likes received: {user.TotalLikes:N0}");
Console.WriteLine($"  Collections: {user.TotalCollections:N0}");
Console.WriteLine($"  Followers: {user.FollowersCount:N0}");

// Profile images
Console.WriteLine($"Profile Images:");
Console.WriteLine($"  Small: {user.ProfileImage.Small}");
Console.WriteLine($"  Medium: {user.ProfileImage.Medium}");
Console.WriteLine($"  Large: {user.ProfileImage.Large}");

// Check if user is available for hire
if (user.ForHire)
{
    Console.WriteLine("✅ Available for hire");
}

// Social media links
if (!string.IsNullOrEmpty(user.TwitterUsername))
{
    Console.WriteLine($"Twitter: @{user.TwitterUsername}");
}
```

### User Analysis

```csharp
public static class UserAnalyzer
{
    public static UserTier GetUserTier(User user)
    {
        return user.TotalPhotos switch
        {
            >= 1000 => UserTier.Professional,
            >= 100 => UserTier.Advanced,
            >= 10 => UserTier.Intermediate,
            _ => UserTier.Beginner
        };
    }
    
    public static double GetEngagementRate(User user)
    {
        return user.TotalPhotos > 0 ? (double)user.TotalLikes / user.TotalPhotos : 0;
    }
    
    public static bool IsInfluencer(User user)
    {
        return user.FollowersCount > 10000 || 
               user.TotalLikes > 100000 || 
               user.TotalPhotos > 500;
    }
    
    public static string GetUserDescription(User user)
    {
        var tier = GetUserTier(user);
        var engagement = GetEngagementRate(user);
        var isInfluencer = IsInfluencer(user);
        
        var description = $"{tier} photographer";
        
        if (engagement > 100)
            description += " with high engagement";
        
        if (isInfluencer)
            description += " and influencer status";
        
        if (user.ForHire)
            description += " (available for hire)";
        
        return description;
    }
}

public enum UserTier
{
    Beginner,
    Intermediate,
    Advanced,
    Professional
}
```

## Collection Model

The `Collection` class represents a curated collection of photos.

### Properties

```csharp
public class Collection : INotifyPropertyChanged
{
    // Basic Information
    public string Id { get; set; }                  // Unique collection identifier
    public string Title { get; set; }              // Collection title
    public string Description { get; set; }        // Collection description
    public string PublishedAt { get; set; }        // Publication timestamp
    public string UpdatedAt { get; set; }          // Last update timestamp
    
    // Metadata
    public int TotalPhotos { get; set; }           // Number of photos in collection
    public bool IsPrivate { get; set; }            // Privacy status
    public string ShareKey { get; set; }           // Share key for private collections
    
    // Complex Properties
    public Photo CoverPhoto { get; set; }          // Collection cover photo
    public User User { get; set; }                // Collection creator
    public CollectionLinks Links { get; set; }     // Related API links
}
```

### Usage Examples

```csharp
// Get collection information
var collection = await client.GetCollectionAsync("499830");

Console.WriteLine($"Collection: {collection.Title}");
Console.WriteLine($"Description: {collection.Description}");
Console.WriteLine($"Created by: {collection.User.Name}");
Console.WriteLine($"Photos: {collection.TotalPhotos:N0}");
Console.WriteLine($"Published: {DateTime.Parse(collection.PublishedAt):yyyy-MM-dd}");
Console.WriteLine($"Privacy: {(collection.IsPrivate ? "Private" : "Public")}");

// Cover photo information
if (collection.CoverPhoto != null)
{
    Console.WriteLine($"Cover Photo:");
    Console.WriteLine($"  By: {collection.CoverPhoto.User.Name}");
    Console.WriteLine($"  URL: {collection.CoverPhoto.Urls.Regular}");
}

// Get photos from collection
var photos = await client.GetCollectionPhotosAsync(collection.Id, perPage: 10);
Console.WriteLine($"\nFirst 10 photos:");
foreach (var photo in photos)
{
    Console.WriteLine($"  - {photo.Description ?? "Untitled"} by {photo.User.Name}");
}
```

## URL Models

### Urls Class

The `Urls` class provides different sizes and formats of photo URLs.

```csharp
public class Urls
{
    public string Raw { get; set; }        // Full resolution, uncompressed
    public string Full { get; set; }       // Large size (max 2048px on longest side)
    public string Regular { get; set; }    // Medium size (max 1080px on longest side)
    public string Small { get; set; }      // Small size (max 400px on longest side)
    public string Thumbnail { get; set; }  // Thumbnail (max 200px on longest side)
    public string Custom { get; set; }     // Custom size (when width/height specified)
}
```

### URL Usage Examples

```csharp
public static class PhotoUrlHelper
{
    public static string GetBestUrlForSize(Urls urls, int maxWidth, int maxHeight)
    {
        // Choose the most appropriate URL based on desired dimensions
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

    public static Dictionary<string, string> GetAllSizes(Urls urls)
    {
        return new Dictionary<string, string>
        {
            ["thumbnail"] = urls.Thumbnail,
            ["small"] = urls.Small,
            ["regular"] = urls.Regular,
            ["full"] = urls.Full,
            ["raw"] = urls.Raw
        };
    }

    public static long EstimateFileSize(string url, int width, int height)
    {
        // Rough estimation based on dimensions and compression
        var pixels = width * height;
        var compressionRatio = url.Contains("raw") ? 0.3 : 0.1; // Raw vs compressed

        return (long)(pixels * 3 * compressionRatio); // 3 bytes per pixel (RGB)
    }
}

// Usage example
var photo = await client.GetPhotoAsync("photo-id");

// Get appropriate URL for different use cases
var thumbnailUrl = PhotoUrlHelper.GetBestUrlForSize(photo.Urls, 200, 200);
var heroImageUrl = PhotoUrlHelper.GetBestUrlForSize(photo.Urls, 1920, 1080);
var printQualityUrl = PhotoUrlHelper.GetBestUrlForSize(photo.Urls, 4000, 3000);

// Estimate download sizes
var sizes = PhotoUrlHelper.GetAllSizes(photo.Urls);
foreach (var (sizeName, url) in sizes)
{
    var estimatedSize = PhotoUrlHelper.EstimateFileSize(url, photo.Width, photo.Height);
    Console.WriteLine($"{sizeName}: {url} (~{estimatedSize / 1024:N0} KB)");
}
```

### ProfileImage Class

```csharp
public class ProfileImage
{
    public string Small { get; set; }      // Small profile image (32x32)
    public string Medium { get; set; }     // Medium profile image (64x64)
    public string Large { get; set; }      // Large profile image (128x128)
}
```

## Location and EXIF Models

### Location Class

```csharp
public class Location
{
    public string Name { get; set; }       // Location name (e.g., "Paris, France")
    public string City { get; set; }       // City name
    public string Country { get; set; }    // Country name
    public Position Position { get; set; } // GPS coordinates
}

public class Position
{
    public double Latitude { get; set; }   // GPS latitude
    public double Longitude { get; set; }  // GPS longitude
}
```

### EXIF Class

```csharp
public class Exif
{
    public string Make { get; set; }           // Camera manufacturer (e.g., "Canon")
    public string Model { get; set; }         // Camera model (e.g., "EOS 5D Mark IV")
    public string ExposureTime { get; set; }  // Shutter speed (e.g., "1/125")
    public string Aperture { get; set; }      // F-stop (e.g., "2.8")
    public int Iso { get; set; }              // ISO sensitivity
    public string FocalLength { get; set; }   // Focal length (e.g., "85mm")
}
```

### Location and EXIF Usage Examples

```csharp
// Analyze photo location data
public static class LocationAnalyzer
{
    public static bool HasLocationData(Photo photo)
    {
        return photo.Location != null &&
               (!string.IsNullOrEmpty(photo.Location.Name) ||
                photo.Location.Position != null);
    }

    public static double CalculateDistance(Position pos1, Position pos2)
    {
        // Haversine formula for distance calculation
        const double R = 6371; // Earth's radius in kilometers

        var lat1Rad = pos1.Latitude * Math.PI / 180;
        var lat2Rad = pos2.Latitude * Math.PI / 180;
        var deltaLatRad = (pos2.Latitude - pos1.Latitude) * Math.PI / 180;
        var deltaLonRad = (pos2.Longitude - pos1.Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    public static string GetLocationSummary(Location location)
    {
        if (string.IsNullOrEmpty(location.Name))
            return "Location not specified";

        var summary = location.Name;

        if (location.Position != null)
        {
            summary += $" ({location.Position.Latitude:F4}, {location.Position.Longitude:F4})";
        }

        return summary;
    }
}

// Analyze camera EXIF data
public static class ExifAnalyzer
{
    public static bool HasExifData(Photo photo)
    {
        return photo.Exif != null && !string.IsNullOrEmpty(photo.Exif.Make);
    }

    public static string GetCameraInfo(Exif exif)
    {
        if (string.IsNullOrEmpty(exif.Make))
            return "Camera information not available";

        return $"{exif.Make} {exif.Model}".Trim();
    }

    public static string GetCameraSettings(Exif exif)
    {
        var settings = new List<string>();

        if (!string.IsNullOrEmpty(exif.Aperture))
            settings.Add($"f/{exif.Aperture}");

        if (!string.IsNullOrEmpty(exif.ExposureTime))
            settings.Add($"{exif.ExposureTime}s");

        if (exif.Iso > 0)
            settings.Add($"ISO {exif.Iso}");

        if (!string.IsNullOrEmpty(exif.FocalLength))
            settings.Add(exif.FocalLength);

        return settings.Count > 0 ? string.Join(", ", settings) : "Settings not available";
    }

    public static CameraType GetCameraType(Exif exif)
    {
        if (string.IsNullOrEmpty(exif.Make))
            return CameraType.Unknown;

        var make = exif.Make.ToLowerInvariant();

        return make switch
        {
            var m when m.Contains("canon") => CameraType.Canon,
            var m when m.Contains("nikon") => CameraType.Nikon,
            var m when m.Contains("sony") => CameraType.Sony,
            var m when m.Contains("fuji") => CameraType.Fujifilm,
            var m when m.Contains("olympus") => CameraType.Olympus,
            var m when m.Contains("panasonic") => CameraType.Panasonic,
            var m when m.Contains("leica") => CameraType.Leica,
            _ => CameraType.Other
        };
    }
}

public enum CameraType
{
    Unknown,
    Canon,
    Nikon,
    Sony,
    Fujifilm,
    Olympus,
    Panasonic,
    Leica,
    Other
}

// Usage example
var photo = await client.GetPhotoAsync("photo-id");

// Location analysis
if (LocationAnalyzer.HasLocationData(photo))
{
    Console.WriteLine($"Location: {LocationAnalyzer.GetLocationSummary(photo.Location)}");
}

// EXIF analysis
if (ExifAnalyzer.HasExifData(photo))
{
    Console.WriteLine($"Camera: {ExifAnalyzer.GetCameraInfo(photo.Exif)}");
    Console.WriteLine($"Settings: {ExifAnalyzer.GetCameraSettings(photo.Exif)}");
    Console.WriteLine($"Brand: {ExifAnalyzer.GetCameraType(photo.Exif)}");
}
```

## Statistics Models

### UnplashTotalStats Class

```csharp
public class UnplashTotalStats
{
    public long Photos { get; set; }              // Total photos on Unsplash
    public long Downloads { get; set; }           // Total downloads
    public long Views { get; set; }               // Total views
    public long Likes { get; set; }               // Total likes
    public long Photographers { get; set; }       // Total photographers
    public long PixelsServed { get; set; }        // Total pixels served
    public long ViewsThisMonth { get; set; }      // Views in current month
    public long NewPhotosThisMonth { get; set; }  // New photos this month
}
```

### UnplashMonthlyStats Class

```csharp
public class UnplashMonthlyStats
{
    public long Downloads { get; set; }           // Downloads this month
    public long Views { get; set; }               // Views this month
    public long Likes { get; set; }               // Likes this month
    public long NewPhotos { get; set; }           // New photos this month
    public long NewPhotographers { get; set; }    // New photographers this month
    public long NewPixels { get; set; }           // New pixels this month
    public long NewDevelopers { get; set; }       // New developers this month
    public long NewApplications { get; set; }     // New applications this month
    public long NewRequests { get; set; }         // New API requests this month
}
```

### Statistics Usage Examples

```csharp
// Get and analyze platform statistics
public static class StatsAnalyzer
{
    public static async Task AnalyzePlatformGrowth(UnsplasharpClient client)
    {
        var totalStats = await client.GetTotalStatsAsync();
        var monthlyStats = await client.GetMonthlyStatsAsync();

        Console.WriteLine("=== Unsplash Platform Statistics ===");
        Console.WriteLine($"Total Photos: {totalStats.Photos:N0}");
        Console.WriteLine($"Total Photographers: {totalStats.Photographers:N0}");
        Console.WriteLine($"Total Downloads: {totalStats.Downloads:N0}");
        Console.WriteLine($"Total Views: {totalStats.Views:N0}");

        // Calculate averages
        var avgPhotosPerPhotographer = (double)totalStats.Photos / totalStats.Photographers;
        var avgDownloadsPerPhoto = (double)totalStats.Downloads / totalStats.Photos;
        var avgViewsPerPhoto = (double)totalStats.Views / totalStats.Photos;

        Console.WriteLine($"\n=== Platform Averages ===");
        Console.WriteLine($"Photos per photographer: {avgPhotosPerPhotographer:F1}");
        Console.WriteLine($"Downloads per photo: {avgDownloadsPerPhoto:F1}");
        Console.WriteLine($"Views per photo: {avgViewsPerPhoto:F1}");

        // Monthly growth analysis
        Console.WriteLine($"\n=== Monthly Growth ===");
        Console.WriteLine($"New photos: {monthlyStats.NewPhotos:N0}");
        Console.WriteLine($"New photographers: {monthlyStats.NewPhotographers:N0}");
        Console.WriteLine($"New developers: {monthlyStats.NewDevelopers:N0}");
        Console.WriteLine($"New applications: {monthlyStats.NewApplications:N0}");

        // Growth rates (approximate)
        var monthlyPhotoGrowthRate = (double)monthlyStats.NewPhotos / totalStats.Photos * 100;
        var monthlyPhotographerGrowthRate = (double)monthlyStats.NewPhotographers / totalStats.Photographers * 100;

        Console.WriteLine($"\n=== Growth Rates (Monthly) ===");
        Console.WriteLine($"Photo growth: {monthlyPhotoGrowthRate:F2}%");
        Console.WriteLine($"Photographer growth: {monthlyPhotographerGrowthRate:F2}%");
    }

    public static PlatformHealth AssessPlatformHealth(UnplashTotalStats totalStats, UnplashMonthlyStats monthlyStats)
    {
        var score = 0;

        // Photo volume scoring
        if (totalStats.Photos > 1000000) score += 2;
        else if (totalStats.Photos > 100000) score += 1;

        // Engagement scoring
        var engagementRate = (double)totalStats.Likes / totalStats.Views;
        if (engagementRate > 0.1) score += 2;
        else if (engagementRate > 0.05) score += 1;

        // Growth scoring
        var monthlyGrowthRate = (double)monthlyStats.NewPhotos / totalStats.Photos;
        if (monthlyGrowthRate > 0.01) score += 2; // >1% monthly growth
        else if (monthlyGrowthRate > 0.005) score += 1; // >0.5% monthly growth

        return score switch
        {
            >= 5 => PlatformHealth.Excellent,
            >= 3 => PlatformHealth.Good,
            >= 1 => PlatformHealth.Fair,
            _ => PlatformHealth.Poor
        };
    }
}

public enum PlatformHealth
{
    Poor,
    Fair,
    Good,
    Excellent
}
```

## Link Models

### PhotoLinks Class

```csharp
public class PhotoLinks
{
    public string Self { get; set; }              // API endpoint for this photo
    public string Html { get; set; }             // Unsplash.com page for this photo
    public string Download { get; set; }         // Direct download URL
    public string DownloadLocation { get; set; } // Download tracking endpoint
}
```

### UserLinks Class

```csharp
public class UserLinks
{
    public string Self { get; set; }      // API endpoint for this user
    public string Html { get; set; }      // Unsplash.com profile page
    public string Photos { get; set; }    // API endpoint for user's photos
    public string Likes { get; set; }     // API endpoint for user's likes
    public string Portfolio { get; set; } // API endpoint for user's portfolio
    public string Following { get; set; } // API endpoint for users this user follows
    public string Followers { get; set; } // API endpoint for this user's followers
}
```

### CollectionLinks Class

```csharp
public class CollectionLinks
{
    public string Self { get; set; }      // API endpoint for this collection
    public string Html { get; set; }      // Unsplash.com page for this collection
    public string Photos { get; set; }    // API endpoint for collection's photos
    public string Related { get; set; }   // API endpoint for related collections
}
```

## Supporting Models

### Category Class

```csharp
public class Category
{
    public int Id { get; set; }           // Category identifier
    public string Title { get; set; }     // Category title
    public int PhotoCount { get; set; }   // Number of photos in category
    public CategoryLinks Links { get; set; } // Related links
}

public class CategoryLinks
{
    public string Self { get; set; }      // API endpoint for this category
    public string Photos { get; set; }    // API endpoint for category's photos
}
```

### Badge Class

```csharp
public class Badge
{
    public string Title { get; set; }     // Badge title (e.g., "Book contributor")
    public bool Primary { get; set; }     // Whether this is the primary badge
    public string Slug { get; set; }      // Badge slug identifier
    public string Link { get; set; }      // Link related to the badge
}
```

## Model Relationships

### Relationship Diagram

```
Photo
├── User (photographer)
├── Urls (different sizes)
├── Location
│   └── Position (GPS coordinates)
├── Exif (camera data)
├── PhotoLinks (API endpoints)
├── Categories[] (tags/topics)
└── CurrentUserCollection[] (user's collections)

User
├── ProfileImage (avatar URLs)
├── Badge (achievements)
└── UserLinks (API endpoints)

Collection
├── User (creator)
├── CoverPhoto (Photo)
└── CollectionLinks (API endpoints)
```

### Navigation Examples

```csharp
// Navigate from photo to photographer's other work
var photo = await client.GetPhotoAsync("photo-id");
var photographer = photo.User;
var photographerPhotos = await client.GetUserPhotosAsync(photographer.Username);

// Find photos in the same location
if (photo.Location?.Position != null)
{
    var nearbyPhotos = await client.SearchPhotosAsync(
        $"location:{photo.Location.Name}",
        perPage: 20
    );
}

// Get photos with similar camera equipment
if (!string.IsNullOrEmpty(photo.Exif.Make))
{
    var similarCameraPhotos = await client.SearchPhotosAsync(
        $"{photo.Exif.Make} {photo.Exif.Model}",
        perPage: 20
    );
}

// Explore collections containing this photo
foreach (var collection in photo.CurrentUserCollection)
{
    var collectionPhotos = await client.GetCollectionPhotosAsync(collection.Id);
    Console.WriteLine($"Collection '{collection.Title}' has {collectionPhotos.Count} photos");
}
```

## Usage Examples

### Complete Photo Analysis

```csharp
public static async Task AnalyzePhoto(UnsplasharpClient client, string photoId)
{
    var photo = await client.GetPhotoAsync(photoId);

    Console.WriteLine("=== PHOTO ANALYSIS ===");
    Console.WriteLine($"ID: {photo.Id}");
    Console.WriteLine($"Title: {photo.Description ?? "Untitled"}");
    Console.WriteLine($"Dimensions: {photo.Width}x{photo.Height} ({PhotoAnalyzer.GetOrientationDescription(photo)})");
    Console.WriteLine($"Quality: {PhotoAnalyzer.AssessQuality(photo)}");
    Console.WriteLine($"Dominant Color: {photo.Color}");
    Console.WriteLine($"Engagement: {photo.Likes:N0} likes, {photo.Downloads:N0} downloads");

    // Photographer information
    Console.WriteLine($"\n=== PHOTOGRAPHER ===");
    Console.WriteLine($"Name: {photo.User.Name} (@{photo.User.Username})");
    Console.WriteLine($"Profile: {UserAnalyzer.GetUserDescription(photo.User)}");
    Console.WriteLine($"Stats: {photo.User.TotalPhotos:N0} photos, {photo.User.TotalLikes:N0} likes received");

    // Technical details
    if (ExifAnalyzer.HasExifData(photo))
    {
        Console.WriteLine($"\n=== CAMERA INFO ===");
        Console.WriteLine($"Camera: {ExifAnalyzer.GetCameraInfo(photo.Exif)}");
        Console.WriteLine($"Settings: {ExifAnalyzer.GetCameraSettings(photo.Exif)}");
    }

    // Location information
    if (LocationAnalyzer.HasLocationData(photo))
    {
        Console.WriteLine($"\n=== LOCATION ===");
        Console.WriteLine($"Location: {LocationAnalyzer.GetLocationSummary(photo.Location)}");
    }

    // Available URLs
    Console.WriteLine($"\n=== AVAILABLE SIZES ===");
    var sizes = PhotoUrlHelper.GetAllSizes(photo.Urls);
    foreach (var (sizeName, url) in sizes)
    {
        var estimatedSize = PhotoUrlHelper.EstimateFileSize(url, photo.Width, photo.Height);
        Console.WriteLine($"{sizeName}: ~{estimatedSize / 1024:N0} KB");
    }
}
```
```
```
