# Documentation Navigation Guide

This guide helps you navigate the Unsplasharp documentation efficiently based on your needs and experience level.

## 🎯 Find What You Need

### I'm New to Unsplasharp
**Start Here:**
1. [Getting Started Guide](docs/getting-started.md) - Complete setup and first examples
2. [API Key Setup](docs/obtaining-an-api-key.md) - Get your Unsplash credentials
3. [Basic Examples](code-examples.md#basic-operations) - Simple code to get you started
4. [Error Handling Basics](error-handling.md#basic-error-handling) - Handle common errors

**Next Steps:**
- [API Reference](api-reference.md) - Learn about available methods
- [Model Reference](models-reference.md) - Understand data structures

### I Want to Build Something Specific

#### Photo Search Application
1. [Search Examples](code-examples.md#search-and-discovery) - Search implementation
2. [Advanced Search](advanced-usage.md#filtering-and-search-optimization) - Filters and optimization
3. [Pagination](advanced-usage.md#advanced-pagination-strategies) - Handle large result sets
4. [Caching](advanced-usage.md#performance-optimization) - Improve performance

#### Photo Gallery/Viewer
1. [Photo Retrieval](api-reference.md#photo-methods) - Get photos by ID
2. [User Profiles](api-reference.md#user-methods) - Display photographer info
3. [Collections](api-reference.md#collection-methods) - Browse curated collections
4. [Download Images](code-examples.md#image-processing-and-download) - Save photos locally

#### Web Application Integration
1. [ASP.NET Core Examples](code-examples.md#web-application-integration) - Web API integration
2. [Dependency Injection](http-client-factory.md) - Proper DI setup
3. [Caching Strategies](advanced-usage.md#performance-optimization) - Web app caching
4. [Error Handling](error-handling.md) - Web-specific error handling

#### Desktop Application
1. [WPF Examples](code-examples.md#desktop-application-examples) - Desktop UI integration
2. [Console Examples](code-examples.md#desktop-application-examples) - Command-line tools
3. [Background Services](code-examples.md#background-services) - Long-running tasks
4. [Local Storage](code-examples.md#image-processing-and-download) - File management

#### Background/Service Applications
1. [Background Services](code-examples.md#background-services) - Service implementation
2. [Batch Processing](advanced-usage.md#batch-operations) - Handle multiple operations
3. [Rate Limiting](advanced-usage.md#performance-optimization) - Avoid API limits
4. [Monitoring](testing-best-practices.md#monitoring-and-observability) - Track performance

### I'm Having Problems

#### Common Issues
| Problem | Solution |
|---------|----------|
| **API key not working** | [API Key Setup](docs/obtaining-an-api-key.md) |
| **Rate limit errors** | [Rate Limiting Guide](error-handling.md#rate-limit-handling) |
| **Photos not loading** | [Error Handling](error-handling.md) |
| **Slow performance** | [Performance Optimization](advanced-usage.md#performance-optimization) |
| **Memory issues** | [Best Practices](testing-best-practices.md#best-practices) |
| **Testing problems** | [Testing Guide](testing-best-practices.md) |

#### Error Types
- **UnsplasharpNotFoundException**: [Not Found Handling](error-handling.md#handling-not-found-errors)
- **UnsplasharpRateLimitException**: [Rate Limit Handling](error-handling.md#rate-limit-handling)
- **UnsplasharpAuthenticationException**: [Auth Issues](error-handling.md#authentication-errors)
- **UnsplasharpNetworkException**: [Network Problems](error-handling.md#network-errors)

### I Want to Upgrade/Migrate

#### From Older Versions
1. [Migration Guide](migration-guide.md) - Complete upgrade guide
2. [Breaking Changes](migration-guide.md#breaking-changes) - What changed
3. [New Features](migration-guide.md#new-features-overview) - What's new
4. [Migration Strategies](migration-guide.md#migration-strategies) - How to upgrade

#### Adopting New Features
- [Exception Handling](migration-guide.md#error-handling-migration) - New error handling
- [IHttpClientFactory](migration-guide.md#ihttpclientfactory-migration) - Modern HTTP clients
- [Logging](migration-guide.md#logging-integration) - Structured logging
- [Performance](migration-guide.md#performance-improvements) - Speed improvements

### I'm Building for Production

#### Essential Reading
1. [Testing Guide](testing-best-practices.md) - Comprehensive testing
2. [Best Practices](testing-best-practices.md#best-practices) - Production guidelines
3. [Security](testing-best-practices.md#security-considerations) - Secure implementation
4. [Monitoring](testing-best-practices.md#monitoring-and-observability) - Track your app

#### Production Checklist
- [ ] [Error Handling](error-handling.md) implemented
- [ ] [Rate Limiting](advanced-usage.md#performance-optimization) handled
- [ ] [Caching](advanced-usage.md#performance-optimization) configured
- [ ] [Logging](logging.md) set up
- [ ] [Health Checks](testing-best-practices.md#production-deployment) added
- [ ] [Tests](testing-best-practices.md) written
- [ ] [Monitoring](testing-best-practices.md#monitoring-and-observability) configured

## 📖 Documentation Structure

### Core Documentation Files
```
docs/
├── index.md                    # Main documentation hub
├── api-reference.md           # Complete API documentation
├── models-reference.md        # Model classes documentation
├── advanced-usage.md          # Advanced patterns and optimization
├── code-examples.md           # Practical examples and recipes
├── testing-best-practices.md  # Testing and production guidance
├── migration-guide.md         # Version upgrade guide
├── error-handling.md          # Error handling strategies
├── http-client-factory.md     # HTTP client configuration
├── logging.md                 # Logging setup and configuration
└── docs/
    ├── getting-started.md     # Comprehensive getting started guide
    ├── introduction.md        # Library introduction
    ├── obtaining-an-api-key.md # API key setup
    └── downloading-a-photo.md # Photo download examples
```

### Content Organization

#### By Complexity Level
- **Beginner**: `docs/getting-started.md`, `docs/obtaining-an-api-key.md`
- **Intermediate**: `api-reference.md`, `models-reference.md`, `error-handling.md`
- **Advanced**: `advanced-usage.md`, `testing-best-practices.md`, `migration-guide.md`

#### By Topic
- **Setup & Configuration**: Getting started, API keys, HTTP client factory, logging
- **API Usage**: API reference, models, code examples
- **Advanced Topics**: Advanced usage, performance, batch operations
- **Quality & Production**: Testing, best practices, error handling, monitoring
- **Migration & Maintenance**: Migration guide, troubleshooting

## 🔍 Search Tips

### Finding Specific Information
- **Method documentation**: Check [API Reference](api-reference.md)
- **Data structures**: See [Model Reference](models-reference.md)
- **Code examples**: Browse [Code Examples](code-examples.md)
- **Error solutions**: Search [Error Handling](error-handling.md)
- **Performance tips**: Look in [Advanced Usage](advanced-usage.md)

### Using Documentation Search
1. Use browser search (Ctrl+F / Cmd+F) within pages
2. Search for specific method names (e.g., "GetRandomPhoto")
3. Look for error types (e.g., "UnsplasharpRateLimitException")
4. Search by use case (e.g., "download", "search", "cache")

## 🚀 Learning Paths

### Path 1: Quick Start (30 minutes)
1. [Getting Started](docs/getting-started.md) - Setup and first request
2. [Basic Examples](code-examples.md#basic-operations) - Simple operations
3. [Error Handling Basics](error-handling.md#basic-error-handling) - Handle errors

### Path 2: Building an App (2-3 hours)
1. [Getting Started](docs/getting-started.md) - Foundation
2. [API Reference](api-reference.md) - Learn the API
3. [Code Examples](code-examples.md) - Implementation patterns
4. [Error Handling](error-handling.md) - Robust error handling
5. [Testing](testing-best-practices.md) - Quality assurance

### Path 3: Production Ready (1-2 days)
1. Complete Path 2 above
2. [Advanced Usage](advanced-usage.md) - Optimization techniques
3. [Best Practices](testing-best-practices.md#best-practices) - Production guidelines
4. [Security](testing-best-practices.md#security-considerations) - Secure implementation
5. [Monitoring](testing-best-practices.md#monitoring-and-observability) - Observability

### Path 4: Expert Level (Ongoing)
1. Complete Path 3 above
2. [Migration Guide](migration-guide.md) - Stay current
3. [Advanced Patterns](advanced-usage.md) - Complex scenarios
4. [Performance Optimization](advanced-usage.md#performance-optimization) - Maximum efficiency
5. Contribute to documentation and examples

## 📱 Mobile-Friendly Navigation

### Quick Links
- [📖 Getting Started](docs/getting-started.md)
- [🔍 API Reference](api-reference.md)
- [💡 Examples](code-examples.md)
- [⚠️ Error Handling](error-handling.md)
- [🚀 Advanced](advanced-usage.md)
- [🧪 Testing](testing-best-practices.md)

### Bookmarks for Development
Save these for quick reference during development:
- [Method List](api-reference.md#table-of-contents)
- [Error Types](error-handling.md#exception-types)
- [Model Properties](models-reference.md#photo-model)
- [Code Snippets](code-examples.md#basic-operations)

---

**Need help finding something?** Check the [main documentation index](index.md) or [open an issue](https://github.com/rootasjey/unsplasharp/issues) on GitHub.
