# Unsplasharp 📷

> ⚠️ Looking for maintenairs
> I don't have much time to work on this lib.

Unofficial C# wrapper around [Unsplash](https://unsplash.com) API targeting .NET Standard 2.0.

This lib is compatible with .NET Core, .NET Framework 4.6.1+, Xamarin (iOS, Android), Universal Windows Platform.

## ✨ Recent Improvements

This library has been enhanced with modern .NET practices and reliability features:

- **🔄 Async/Await Patterns**: Improved async patterns with proper `ConfigureAwait(false)` usage
- **⏹️ Cancellation Token Support**: All async methods now support `CancellationToken` for request cancellation and timeouts
- **📊 Structured Logging**: Built-in support for Microsoft.Extensions.Logging with detailed request/response logging
- **🔁 Retry Policies**: Automatic retry logic using Polly for resilient HTTP requests
- **⚡ Performance**: Migrated to System.Text.Json for 2-3x faster JSON processing and reduced memory usage
- **🛡️ Comprehensive Error Handling**: Structured exceptions with rich context for better debugging and error recovery
- **🚀 Modern JSON**: System.Text.Json integration with custom converters and safe property access

**Currently incomplete** 🚧

* Missing [user's authentication actions](https://unsplash.com/documentation#user-authentication)

## Installation

[NuGet](https://www.nuget.org/packages/unsplasharp.api/): ```Install-Package unsplasharp.api```

## Usage

### Basic Usage

```csharp
using Unsplasharp;

var client = new UnsplasharpClient("YOUR_APPLICATION_ID");
var photosFound = await client.SearchPhotos("mountains");
```

### Cancellation Token Support

All async methods now support `CancellationToken` for request cancellation and timeout handling:

```csharp
using Unsplasharp;

var client = new UnsplasharpClient("YOUR_APPLICATION_ID");

// Timeout after 30 seconds
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var photo = await client.GetRandomPhoto(cts.Token);

// Manual cancellation
using var cts2 = new CancellationTokenSource();
var searchTask = client.SearchPhotos("nature", 1, 10, cts2.Token);
// Later: cts2.Cancel();

// Integration with ASP.NET Core
public async Task<IActionResult> GetPhoto(string id, CancellationToken cancellationToken)
{
    var photo = await client.GetPhoto(id, 0, 0, cancellationToken);
    return Ok(photo);
}
```

**Supported Methods:**
- All photo methods: `GetPhoto()`, `GetRandomPhoto()`, `ListPhotos()`, `SearchPhotos()`
- All collection methods: `GetCollection()`, `ListCollections()`, `GetCollectionPhotos()`
- All user methods: `GetUser()`
- All search methods: `SearchPhotos()`, `SearchCollections()`
- Stats methods: `GetTotalStats()`

> **🔄 Backward Compatibility**: All existing method signatures remain unchanged. Cancellation token support is added as optional overloads, so your existing code will continue to work without any modifications.

### Advanced Usage with Logging

```csharp
using Unsplasharp;
using Microsoft.Extensions.Logging;

// Create a logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var logger = loggerFactory.CreateLogger<UnsplasharpClient>();

// Create client with logging support
var client = new UnsplasharpClient("YOUR_APPLICATION_ID", logger: logger);

// The client will now log HTTP requests, retries, and errors
var photos = await client.SearchPhotos("nature");
```

### Modern Usage with IHttpClientFactory (Recommended)

For ASP.NET Core and modern .NET applications, use the IHttpClientFactory integration:

```csharp
using Unsplasharp.Extensions;

// In Program.cs or Startup.cs
builder.Services.AddUnsplasharp("YOUR_APPLICATION_ID");

// Or with configuration
builder.Services.AddUnsplasharp(options =>
{
    options.ApplicationId = "YOUR_APPLICATION_ID";
    options.Secret = "YOUR_SECRET"; // Optional
    options.ConfigureHttpClient = client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);
    };
});

// In your controller or service
public class PhotoService
{
    private readonly UnsplasharpClient _client;

    public PhotoService(UnsplasharpClient client)
    {
        _client = client;
    }

    public async Task<List<Photo>> GetPhotos(string query)
    {
        return await _client.SearchPhotos(query);
    }
}
```

### Features

- **🏭 IHttpClientFactory Support**: Modern HTTP client management with proper lifecycle and DI integration
- **🔄 Automatic Retries**: Failed requests are automatically retried up to 3 times with exponential backoff
- **📊 Structured Logging**: Detailed logging of HTTP requests, responses, and retry attempts
- **📈 Rate Limit Tracking**: Built-in rate limit monitoring and logging
- **⚡ Async/Await**: All methods use proper async patterns with `ConfigureAwait(false)`
- **🚀 High Performance JSON**: System.Text.Json for faster parsing and lower memory usage
- **🔧 Dependency Injection**: Seamless integration with .NET DI containers
- **🔙 Backward Compatible**: Existing code continues to work without changes

### Comprehensive Error Handling

Unsplasharp now provides structured exception handling with rich context information for better debugging and error recovery:

```csharp
try {
    // New methods that throw specific exceptions
    var photo = await client.GetRandomPhotoAsync();
    var specificPhoto = await client.GetPhotoAsync("photo-id");
} catch (UnsplasharpNotFoundException ex) {
    // Handle resource not found (404)
    Console.WriteLine($"Photo '{ex.ResourceId}' not found");
} catch (UnsplasharpRateLimitException ex) {
    // Handle rate limiting (429)
    Console.WriteLine($"Rate limit exceeded. Reset at: {ex.RateLimitReset}");
    await Task.Delay(ex.TimeUntilReset ?? TimeSpan.FromMinutes(1));
} catch (UnsplasharpAuthenticationException ex) {
    // Handle authentication errors (401)
    Console.WriteLine("Invalid application ID or access token");
} catch (UnsplasharpNetworkException ex) when (ex.IsRetryable) {
    // Handle retryable network errors
    Console.WriteLine("Temporary network error, will retry automatically");
} catch (UnsplasharpException ex) {
    // Handle any other API errors
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Context: {ex.Context?.ToSummary()}");
}
```

**Key Features:**
- **🎯 Specific Exception Types**: Different exceptions for different error scenarios
- **📋 Rich Error Context**: Correlation IDs, timing, rate limits, and request details
- **🔄 Intelligent Retries**: Automatic retry logic based on error types
- **🔙 Backward Compatibility**: Existing methods still return null/empty on errors
- **📊 Enhanced Logging**: Structured logging with correlation IDs for debugging

For detailed information, see the **[Error Handling Guide](ERROR_HANDLING.md)**.

## Documentation

- **[Error Handling Guide](ERROR_HANDLING.md)** - Comprehensive error handling with structured exceptions
- **[IHttpClientFactory Integration Guide](HTTPCLIENTFACTORY.md)** - Comprehensive guide for modern .NET applications
- **[Logging Guide](LOGGING.md)** - Detailed logging configuration and usage
- **[System.Text.Json Migration Guide](SYSTEMTEXTJSON.md)** - Performance improvements and technical details
- **[Changelog](CHANGELOG.md)** - Version history and breaking changes

## API documentation

[Official API documentation](https://unsplash.com/documentation)

* [General](https://github.com/rootasjey/unsplasharp#general)
* [Photos](https://github.com/rootasjey/unsplasharp#photos)
* [Collections](https://github.com/rootasjey/unsplasharp#collections)
* [Users](https://github.com/rootasjey/unsplasharp#users)
* [Search](https://github.com/rootasjey/unsplasharp#search)
* [Stats](https://github.com/rootasjey/unsplasharp#stats)
* [Custom Requests](https://github.com/rootasjey/unsplasharp#custom-requests)

### Instanciate a new client

It's necessary to instanciate a new client with at least an application ID to start making requests.

```csharp
var client = new Client("YOUR_APPLICATION_ID");
```

### General

#### Rates limits
Unsplash has API requests [rates limits](https://unsplash.com/documentation#rate-limiting).

An Unsplashsharp client has two properties to help you monitor API calls:

Max API calls allowed per hour

* ```MaxRateLimit```

API calls remaining for the current hour
* ```RateLimitRemaining```

```csharp
if (client.RateLimitRemaining == 0) {
  // Warning the user that he's to wait some time
  // before using the app again.
}
```

```csharp
if (client.MaxRateLimit == 50) {
  // Application is in dev mode.
} else if (client.MaxRateLimit == 5000) {
  // Application is in prod mode.
} else { /* Unknown mode */ }
```

### Photos

#### Get a single photo from an id

```csharp
var photo = await client.GetPhoto("TPv9dh822VA");

// get a photo in the specified width and height in pixels
var photoWidthHeight = await client.GetPhoto(id: "TPv9dh822VA", width: 500, height: 500);
```

#### Get a random photo

```csharp
var randomPhoto = await client.GetRandomPhoto();

// using collections' IDs
var randomPhotoFromCollections = await client.GetRandomPhoto(new string[] { "499830", "194162" });

// from a specific user
var randomPhotoFromUser = await client.GetRandomPhoto(count: 1, username: "matthewkane");

var randomPhotosFromQuery = await client.GetRandomPhoto(count: 3, query:"woman");
```

#### Get a list of all photos

```csharp
var listPhotos = await client.ListPhotos();

var listPhotosPaged = await client.ListPhotos(page:2, perPage:15, orderBy: OrderBy.Popular);
```

### Collections

#### Get a single collection from an id
```csharp
var collection = await client.GetCollection("771520");
```

#### Get a list of all collections
```csharp
var listCollection = await client.ListCollections();
```

#### Get a list of featured collections
```csharp
var listFeaturedCollection = await client.ListFeaturedCollections();
```
#### Get a collection's photos from a collection's id
```csharp
var listPhotos = await client.GetCollectionPhotos("771520");
```

#### Get related collections from a collection's id

```csharp
var collectionsRelated = await client.ListRelatedCollections("771520");
```


### Users

#### Get a single user from his/her username

```csharp
var user = await client.GetUser("unsplash");

var userCustomProfileImage = client.GetUser("seteales", width: 100, height: 100);
```

#### Get a list of user's collections

```csharp
var userCollections = await client.ListUserCollections("unsplash");
```

#### Get a list of user's photos

```csharp
var userPhotos = await client.ListUserPhotos("seteales");

var userPhotosCustomParam = await client.ListUserPhotos("seteales", page: 2, perPage: 2, stats: true);
```

#### Get a list of user's liked photos

```csharp
var userLikedPhotos = await client.ListUserLikedPhotos("seteales");
```

#### Get an user's statistics

```csharp
var userStats = await client.GetUserStats("seteales");
```


### Search
#### Search photos from a query

```csharp
var photosFound = await client.SearchPhoto("mountains");
```

#### Search collections from a query

```csharp
var collectionsFound = await client.SearchCollections("mountains");
```

#### Search users from a query

```csharp
var usersFound = await client.SearchUsers("seteales");
```

### Stats
#### Get Unsplash [total stats](https://unsplash.com/documentation#totals)

```csharp
var totalStats = await client.GetTotalStats();
```

#### Get Unsplash [monthly stats](https://unsplash.com/documentation#month)

```csharp
var monthlyStats = await client.GetMonthlyStats();
```

### Custom Requests

In adition to the previous API methods, you can build and use custom URL's to fetch photos, photos' lists, and collections' lists.

There're also methods to search for collections, photos and users using a custom URL.

#### Fetch a photo

```csharp
var photo = await client.FetchPhoto("you_custom_url");
```

#### Fetch a list of photos

```csharp
var photos = await client.FetchPhotosList("you_custom_url");
```

#### Fetch a list of collections

```csharp
var collections = await client.FetchCollectionsList("you_custom_url");
```

#### Search for photos using a specific search URL

```csharp
var photosFound = await client.FetchSearchPhotosList("your_custom_url");
```

#### Search for collections using a specific search URL

```csharp
var collectionsFound = await client.FetchSearcCollectionsList("your_custom_url");
```

#### Search for users using a specific search URL

```csharp
var usersFound = await client.FetchSearcUsersList("your_custom_url");
```


## Tests

Tests are under [UnsplashsharpTests](https://github.com/rootasjey/unsplasharp/tree/master/UnsplashsharpTest) project.

They check the Unsplash [API status](https://status.unsplash.com/) and that every methods in the lib works properly.

In this project, a dev API key is used which is limited to 50 requests per hour. So ensure you're not off limit.

## Personal API key

If you want to get your personal API key from Unsplash:

1. Go to [Unsplash](https://unsplash.com)
2. Log in or create a new account
3. In the top bar, click on _'API/Developers'_
4. Go to _['Your applications'](https://unsplash.com/oauth/applications)_
5. Click on _'New Application'_ to create a new one and get an API key (and a Secret).

## Dependencies

* **System.Text.Json** - High-performance JSON serialization (included in .NET Standard 2.0+)
* **Microsoft.Extensions.Logging.Abstractions** - Structured logging support
* **Microsoft.Extensions.Http** - IHttpClientFactory integration
* **Polly** - Resilience and retry policies

### Removed Dependencies
* ~~Newtonsoft.Json~~ - Replaced with System.Text.Json for better performance

## Resources

* [Official Unsplash documentation](https://unsplash.com/documentation)

## TODO

* Add [user's authentication actions](https://unsplash.com/documentation#user-authentication)
