---
_layout: landing
---

# Unsplasharp Documentation

A modern, asynchronous, and feature-rich .NET library for the [Unsplash API](https://unsplash.com/developers). This comprehensive documentation will help you get started quickly and make the most of Unsplasharp's powerful features.

## 🚀 Quick Start

New to Unsplasharp? Start here:

1. **[Getting Started Guide](~/docs/getting-started.md)** - Complete setup guide with examples
2. **[API Key Setup](~/docs/obtaining-an-api-key.md)** - How to get your Unsplash API credentials
3. **[Basic Examples](~/code-examples.md#basic-operations)** - Simple code examples to get you started

## 📚 Core Documentation

### Essential Guides
- **[Getting Started](~/docs/getting-started.md)** - Comprehensive setup and first steps
- **[API Reference](~/api-reference.md)** - Complete method documentation with examples
- **[Model Reference](~/models-reference.md)** - Detailed model class documentation
- **[Error Handling](~/error-handling.md)** - Comprehensive error handling strategies

### Advanced Topics
- **[Advanced Usage Patterns](~/advanced-usage.md)** - Pagination, filtering, and optimization
- **[Code Examples & Recipes](~/code-examples.md)** - Practical examples for common scenarios
- **[Testing & Best Practices](~/testing-best-practices.md)** - Testing strategies and production tips
- **[Migration Guide](~/migration-guide.md)** - Upgrading from older versions

### Integration & Configuration
- **[IHttpClientFactory Integration](~/http-client-factory.md)** - Modern HTTP client management
- **[Logging Configuration](~/logging.md)** - Structured logging setup
- **[Downloading Photos](~/docs/downloading-a-photo.md)** - Image download examples

### Navigation & Reference
- **[Quick Reference](~/quick-reference.md)** - Fast reference for common operations
- **[Navigation Guide](~/navigation.md)** - Find what you need quickly
- **[Table of Contents](~/table-of-contents.md)** - Complete documentation overview

## 🎯 Quick Navigation

### By Use Case
| I want to... | Go to |
|--------------|-------|
| **Get started quickly** | [Getting Started Guide](~/docs/getting-started.md) |
| **Search for photos** | [Search Examples](~/code-examples.md#search-and-discovery) |
| **Handle errors properly** | [Error Handling Guide](~/error-handling.md) |
| **Download images** | [Download Examples](~/code-examples.md#image-processing-and-download) |
| **Build a web app** | [Web Integration](~/code-examples.md#web-application-integration) |
| **Test my code** | [Testing Guide](~/testing-best-practices.md) |
| **Optimize performance** | [Advanced Patterns](~/advanced-usage.md#performance-optimization) |
| **Migrate from older version** | [Migration Guide](~/migration-guide.md) |

### By Experience Level
| Level | Recommended Reading |
|-------|-------------------|
| **Beginner** | [Getting Started](~/docs/getting-started.md) → [Basic Examples](~/code-examples.md#basic-operations) → [Error Handling](~/error-handling.md) |
| **Intermediate** | [API Reference](~/api-reference.md) → [Advanced Patterns](~/advanced-usage.md) → [Model Reference](~/models-reference.md) |
| **Advanced** | [Testing Guide](~/testing-best-practices.md) → [Performance Optimization](~/advanced-usage.md#performance-optimization) → [Migration Guide](~/migration-guide.md) |

## 🔧 API Overview

Unsplasharp provides access to all major Unsplash API endpoints:

### Core Features
- **Photos**: Get random photos, search, retrieve by ID
- **Collections**: Browse curated collections
- **Users**: Access photographer profiles and portfolios
- **Search**: Powerful search with filters and pagination
- **Statistics**: Platform usage statistics

### Modern .NET Features
- **Async/Await**: Full async support with cancellation tokens
- **Exception Handling**: Specific exception types with rich context
- **Dependency Injection**: Built-in DI container support
- **IHttpClientFactory**: Proper HTTP client lifecycle management
- **Structured Logging**: Microsoft.Extensions.Logging integration
- **Performance**: Connection pooling and intelligent caching

## 🎨 Code Examples

### Quick Example
```csharp
using Unsplasharp;

// Create client
var client = new UnsplasharpClient("YOUR_APPLICATION_ID");

// Get a random photo
try
{
    var photo = await client.GetRandomPhotoAsync();
    Console.WriteLine($"Photo by {photo.User.Name}: {photo.Urls.Regular}");
}
catch (UnsplasharpException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Search Photos
```csharp
// Search with filters
var photos = await client.SearchPhotosAsync(
    query: "mountain landscape",
    orientation: Orientation.Landscape,
    color: "blue",
    perPage: 20
);

foreach (var photo in photos)
{
    Console.WriteLine($"{photo.Description} by {photo.User.Name}");
}
```

More examples: **[Code Examples & Recipes](~/code-examples.md)**

## 🛠️ Installation

### Package Manager Console
```powershell
Install-Package Unsplasharp
```

### .NET CLI
```bash
dotnet add package Unsplasharp
```

### PackageReference
```xml
<PackageReference Include="Unsplasharp" Version="*" />
```

## 🔗 External Resources

- **[Unsplash API Documentation](https://unsplash.com/documentation)** - Official API docs
- **[Unsplash Developer Guidelines](https://help.unsplash.com/en/articles/2511245-unsplash-api-guidelines)** - API usage guidelines
- **[GitHub Repository](https://github.com/rootasjey/unsplasharp)** - Source code and issues
- **[NuGet Package](https://www.nuget.org/packages/Unsplasharp/)** - Package downloads

## 💡 Need Help?

- **Getting Started Issues**: Check the [Getting Started Guide](~/docs/getting-started.md)
- **API Questions**: See the [API Reference](~/api-reference.md)
- **Error Handling**: Review [Error Handling Guide](~/error-handling.md)
- **Performance Issues**: Read [Advanced Usage Patterns](~/advanced-usage.md)
- **Bug Reports**: Use [GitHub Issues](https://github.com/rootasjey/unsplasharp/issues)

## 📝 Contributing

Unsplasharp is an open-source project. Contributions are welcome! Please see the [contribution guidelines](https://github.com/rootasjey/unsplasharp/blob/main/CONTRIBUTING.md) for more information.

---

**Ready to get started?** → [Getting Started Guide](~/docs/getting-started.md)