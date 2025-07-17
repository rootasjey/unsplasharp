# Unsplasharp Documentation - Table of Contents

Complete overview of all Unsplasharp documentation with detailed section breakdowns.

## 📋 Complete Documentation Index

### 🏠 [Main Documentation Hub](index.md)
- Quick start guide
- Core documentation overview
- Navigation by use case and experience level
- API overview and features
- Installation instructions
- External resources

### 🚀 [Getting Started Guide](docs/getting-started.md)
- Prerequisites and installation
- Getting your API key
- Basic setup (Console, ASP.NET Core)
- Your first request
- Common use cases with examples
- Error handling introduction
- Best practices overview
- Next steps and sample projects

### 🔧 [API Reference Guide](api-reference.md)
- **Client Initialization**
  - Basic initialization
  - Dependency injection setup
- **Photo Methods**
  - GetPhoto / GetPhotoAsync
  - GetRandomPhoto / GetRandomPhotoAsync
  - ListPhotos / ListPhotosAsync
- **Search Methods**
  - SearchPhotos / SearchPhotosAsync
- **Collection Methods**
  - GetCollection / GetCollectionAsync
  - ListCollections / ListCollectionsAsync
  - GetCollectionPhotos / GetCollectionPhotosAsync
  - SearchCollections / SearchCollectionsAsync
- **User Methods**
  - GetUser / GetUserAsync
  - GetUserPhotos / GetUserPhotosAsync
  - GetUserLikes / GetUserLikesAsync
  - GetUserCollections / GetUserCollectionsAsync
- **Statistics Methods**
  - GetTotalStats / GetTotalStatsAsync
  - GetMonthlyStats / GetMonthlyStatsAsync
- **Method Parameters**
  - Common parameters
  - Photo-specific parameters
  - Search filters
- **Return Types**
  - Photo model
  - Collection model
  - User model
  - URL types
- **Rate Limiting**
  - Rate limit information
  - Handling rate limits
- **Best Practices**
  - Exception-throwing methods
  - Error handling patterns
  - Cancellation tokens
  - Rate limit monitoring

### 📊 [Models Reference Guide](models-reference.md)
- **Photo Model**
  - Properties and usage examples
  - Photo filtering and analysis
- **User Model**
  - Properties and usage examples
  - User analysis utilities
- **Collection Model**
  - Properties and usage examples
- **URL Models**
  - Urls class
  - ProfileImage class
  - URL optimization helpers
- **Location and EXIF Models**
  - Location class with GPS data
  - EXIF class with camera data
  - Analysis utilities
- **Statistics Models**
  - UnplashTotalStats class
  - UnplashMonthlyStats class
  - Usage examples
- **Link Models**
  - PhotoLinks, UserLinks, CollectionLinks
- **Supporting Models**
  - Category, Badge classes
- **Model Relationships**
  - Relationship diagram
  - Navigation examples
- **Usage Examples**
  - Complete photo analysis

### 🚀 [Advanced Usage Patterns](advanced-usage.md)
- **Advanced Pagination Strategies**
  - Infinite scroll implementation
  - Parallel pagination
- **Filtering and Search Optimization**
  - Advanced search builder
  - Smart search with fallbacks
- **Custom Parameters and URL Manipulation**
  - Custom photo sizing
  - URL parameter optimization
- **Performance Optimization**
  - Intelligent caching strategy
  - Connection pooling and HTTP optimization
- **Batch Operations**
  - Efficient bulk photo processing
  - Batch download manager
- **Monitoring and Metrics**
  - Performance metrics collection

### 💻 [Code Examples and Recipes](code-examples.md)
- **Basic Operations**
  - Simple photo retrieval
  - Photo information display
- **Search and Discovery**
  - Smart search with fallbacks
  - Advanced search filters
  - Pagination helper
- **User and Collection Management**
  - User profile analysis
  - Collection explorer
- **Image Processing and Download**
  - Smart image downloader
  - Image metadata extractor
- **Web Application Integration**
  - ASP.NET Core photo API
- **Desktop Application Examples**
  - WPF photo gallery
  - Console photo browser
- **Background Services**
  - Photo sync service
- **Testing Patterns**
  - Unit testing with mocking
  - Integration testing
- **Performance Optimization**
  - Caching strategies
  - Batch processing
  - Connection pool optimization

### 🧪 [Testing and Best Practices Guide](testing-best-practices.md)
- **Testing Strategies**
  - Test pyramid approach
  - Testing infrastructure setup
- **Unit Testing**
  - Success scenarios
  - Error scenarios
  - Caching behavior
- **Integration Testing**
  - API contract testing
  - Rate limit testing
- **Performance Testing**
  - Load testing
  - Memory usage testing
- **Best Practices**
  - Error handling patterns
  - Caching strategies
  - Rate limiting approaches
  - Dependency injection
- **Common Pitfalls**
  - Rate limit handling mistakes
  - HttpClient usage issues
  - Cancellation token omissions
- **Security Considerations**
  - API key management
  - Input validation
  - User rate limiting
- **Production Deployment**
  - Configuration management
  - Health checks
  - Graceful degradation
- **Monitoring and Observability**
  - Structured logging
  - Custom metrics
  - Application Insights integration

### 🔄 [Migration and Upgrade Guide](migration-guide.md)
- **Version Compatibility**
  - Supported .NET versions
  - API compatibility
- **Breaking Changes** (None!)
- **New Features Overview**
  - Comprehensive error handling
  - IHttpClientFactory integration
  - Structured logging
  - Enhanced async support
- **Migration Strategies**
  - Gradual migration (recommended)
  - New project setup
- **Error Handling Migration**
  - Before and after examples
  - Migration helper utilities
- **IHttpClientFactory Migration**
  - Manual vs. DI setup
- **Logging Integration**
  - Setup and configuration
- **Performance Improvements**
  - Connection pooling
  - Retry policies
  - Rate limit optimization
- **Best Practices Updates**
  - Cancellation tokens
  - Error handling patterns
  - Dependency injection
  - Caching strategies
- **Troubleshooting Common Issues**
  - Null reference exceptions
  - HttpClient disposal issues
  - Rate limit handling
  - Configuration problems
- **Migration Checklist**
- **Testing Your Migration**

### ⚠️ [Error Handling Guide](error-handling.md)
- **Exception Types**
  - UnsplasharpException (base)
  - UnsplasharpNotFoundException
  - UnsplasharpRateLimitException
  - UnsplasharpAuthenticationException
  - UnsplasharpNetworkException
  - UnsplasharpTimeoutException
- **Error Context**
  - Rich error information
  - Correlation IDs
  - Request/response details
- **Handling Strategies**
  - Basic error handling
  - Advanced error handling
  - Retry patterns
- **Rate Limit Handling**
  - Detection and response
  - Backoff strategies
- **Network Error Handling**
  - Transient vs. permanent errors
  - Retry logic
- **Authentication Errors**
  - API key validation
  - Permission issues
- **Best Practices**
  - Exception-throwing methods
  - Logging strategies
  - User experience considerations

### 🌐 [IHttpClientFactory Integration](http-client-factory.md)
- **Overview and Benefits**
- **Basic Setup**
- **Advanced Configuration**
- **Dependency Injection**
- **Custom HTTP Handlers**
- **Performance Considerations**
- **Troubleshooting**

### 📝 [Logging Configuration](logging.md)
- **Setup and Configuration**
- **Log Levels and Categories**
- **Structured Logging**
- **Custom Log Providers**
- **Performance Considerations**
- **Troubleshooting**

### 📖 [Introduction](docs/introduction.md)
- Library overview
- Key features
- Architecture
- Getting started

### 🔑 [Obtaining an API Key](docs/obtaining-an-api-key.md)
- Unsplash developer account setup
- Application registration
- API key management
- Rate limits and quotas

### 📥 [Downloading a Photo](docs/downloading-a-photo.md)
- Basic download examples
- Different image sizes
- Error handling
- Best practices

### 🧭 [Navigation Guide](navigation.md)
- Find what you need quickly
- Documentation structure
- Learning paths
- Search tips

## 📚 Documentation Categories

### By Audience
- **Beginners**: Getting Started, Introduction, API Key Setup
- **Developers**: API Reference, Models, Code Examples
- **Advanced Users**: Advanced Usage, Testing, Migration
- **DevOps/Production**: Best Practices, Error Handling, Monitoring

### By Topic
- **Setup & Configuration**: Getting Started, HTTP Client Factory, Logging
- **API Usage**: API Reference, Models, Error Handling
- **Development**: Code Examples, Testing, Best Practices
- **Advanced Topics**: Advanced Usage, Performance, Migration
- **Production**: Testing, Security, Monitoring, Deployment

### By Content Type
- **Guides**: Step-by-step instructions and tutorials
- **References**: Complete API and model documentation
- **Examples**: Practical code samples and recipes
- **Best Practices**: Recommendations and patterns

## 🔗 Cross-References

### Common Workflows
1. **New Project Setup**: Getting Started → API Key → Basic Examples → Error Handling
2. **Production Deployment**: Best Practices → Testing → Error Handling → Monitoring
3. **Performance Optimization**: Advanced Usage → Caching → Batch Operations → Monitoring
4. **Troubleshooting**: Error Handling → Testing → Migration Guide → Navigation

### Related Topics
- **Error Handling** ↔ **Testing** ↔ **Best Practices**
- **API Reference** ↔ **Models** ↔ **Code Examples**
- **Advanced Usage** ↔ **Performance** ↔ **Production**
- **Migration** ↔ **Best Practices** ↔ **Testing**

---

**Quick Navigation**: [Main Hub](index.md) | [Getting Started](docs/getting-started.md) | [API Reference](api-reference.md) | [Examples](code-examples.md)
