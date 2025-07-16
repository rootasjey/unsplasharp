using System;
using System.Net;
using System.Net.Http;

namespace Unsplasharp.Exceptions
{
    /// <summary>
    /// Base exception class for all Unsplasharp-related errors
    /// </summary>
    public abstract partial class UnsplasharpException : Exception
    {
        /// <summary>
        /// The request URL that caused the error
        /// </summary>
        public string? RequestUrl { get; }

        /// <summary>
        /// The HTTP method used for the request
        /// </summary>
        public string? HttpMethod { get; }

        /// <summary>
        /// Additional context information about the error
        /// </summary>
        public ErrorContext? Context { get; }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpException class
        /// </summary>
        /// <param name="message">The error message</param>
        protected UnsplasharpException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        protected UnsplasharpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpException class with context
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="context">Additional error context</param>
        protected UnsplasharpException(string message, string? requestUrl, string? httpMethod, ErrorContext? context = null) 
            : base(message)
        {
            RequestUrl = requestUrl;
            HttpMethod = httpMethod;
            Context = context;
        }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpException class with context and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="context">Additional error context</param>
        protected UnsplasharpException(string message, Exception innerException, string? requestUrl, string? httpMethod, ErrorContext? context = null) 
            : base(message, innerException)
        {
            RequestUrl = requestUrl;
            HttpMethod = httpMethod;
            Context = context;
        }
    }

    /// <summary>
    /// Exception thrown when an HTTP request fails
    /// </summary>
    public class UnsplasharpHttpException : UnsplasharpException
    {
        /// <summary>
        /// The HTTP status code returned by the API
        /// </summary>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>
        /// The response content from the API
        /// </summary>
        public string? ResponseContent { get; }

        /// <summary>
        /// Indicates whether this error is retryable
        /// </summary>
        public bool IsRetryable { get; }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpHttpException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="statusCode">The HTTP status code</param>
        /// <param name="responseContent">The response content</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="isRetryable">Whether this error is retryable</param>
        /// <param name="context">Additional error context</param>
        public UnsplasharpHttpException(string message, HttpStatusCode? statusCode, string? responseContent, 
            string? requestUrl, string? httpMethod, bool isRetryable = false, ErrorContext? context = null)
            : base(message, requestUrl, httpMethod, context)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
            IsRetryable = isRetryable;
        }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpHttpException class with inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        /// <param name="statusCode">The HTTP status code</param>
        /// <param name="responseContent">The response content</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="isRetryable">Whether this error is retryable</param>
        /// <param name="context">Additional error context</param>
        public UnsplasharpHttpException(string message, Exception innerException, HttpStatusCode? statusCode, 
            string? responseContent, string? requestUrl, string? httpMethod, bool isRetryable = false, ErrorContext? context = null)
            : base(message, innerException, requestUrl, httpMethod, context)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
            IsRetryable = isRetryable;
        }
    }

    /// <summary>
    /// Exception thrown when the API rate limit is exceeded
    /// </summary>
    public class UnsplasharpRateLimitException : UnsplasharpHttpException
    {
        /// <summary>
        /// The current rate limit
        /// </summary>
        public int? RateLimit { get; }

        /// <summary>
        /// The remaining requests in the current window
        /// </summary>
        public int? RateLimitRemaining { get; }

        /// <summary>
        /// When the rate limit resets (Unix timestamp)
        /// </summary>
        public DateTimeOffset? RateLimitReset { get; }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpRateLimitException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="rateLimit">The current rate limit</param>
        /// <param name="rateLimitRemaining">The remaining requests</param>
        /// <param name="rateLimitReset">When the rate limit resets</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="context">Additional error context</param>
        public UnsplasharpRateLimitException(string message, int? rateLimit, int? rateLimitRemaining, 
            DateTimeOffset? rateLimitReset, string? requestUrl, string? httpMethod, ErrorContext? context = null)
            : base(message, (HttpStatusCode)429, null, requestUrl, httpMethod, false, context)
        {
            RateLimit = rateLimit;
            RateLimitRemaining = rateLimitRemaining;
            RateLimitReset = rateLimitReset;
        }
    }

    /// <summary>
    /// Exception thrown when authentication fails
    /// </summary>
    public class UnsplasharpAuthenticationException : UnsplasharpHttpException
    {
        /// <summary>
        /// Initializes a new instance of the UnsplasharpAuthenticationException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="context">Additional error context</param>
        public UnsplasharpAuthenticationException(string message, string? requestUrl, string? httpMethod, ErrorContext? context = null)
            : base(message, HttpStatusCode.Unauthorized, null, requestUrl, httpMethod, false, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a requested resource is not found
    /// </summary>
    public class UnsplasharpNotFoundException : UnsplasharpHttpException
    {
        /// <summary>
        /// The resource identifier that was not found
        /// </summary>
        public string? ResourceId { get; }

        /// <summary>
        /// The type of resource that was not found
        /// </summary>
        public string? ResourceType { get; }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpNotFoundException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="resourceId">The resource identifier</param>
        /// <param name="resourceType">The resource type</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="context">Additional error context</param>
        public UnsplasharpNotFoundException(string message, string? resourceId, string? resourceType, 
            string? requestUrl, string? httpMethod, ErrorContext? context = null)
            : base(message, HttpStatusCode.NotFound, null, requestUrl, httpMethod, false, context)
        {
            ResourceId = resourceId;
            ResourceType = resourceType;
        }
    }

    /// <summary>
    /// Exception thrown when JSON parsing fails
    /// </summary>
    public class UnsplasharpParsingException : UnsplasharpException
    {
        /// <summary>
        /// The raw response content that failed to parse
        /// </summary>
        public string? RawContent { get; }

        /// <summary>
        /// The expected data type
        /// </summary>
        public string? ExpectedType { get; }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpParsingException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        /// <param name="rawContent">The raw content that failed to parse</param>
        /// <param name="expectedType">The expected data type</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="context">Additional error context</param>
        public UnsplasharpParsingException(string message, Exception innerException, string? rawContent, 
            string? expectedType, string? requestUrl, string? httpMethod, ErrorContext? context = null)
            : base(message, innerException, requestUrl, httpMethod, context)
        {
            RawContent = rawContent;
            ExpectedType = expectedType;
        }
    }

    /// <summary>
    /// Exception thrown when a network error occurs
    /// </summary>
    public class UnsplasharpNetworkException : UnsplasharpException
    {
        /// <summary>
        /// Indicates whether this error is retryable
        /// </summary>
        public bool IsRetryable { get; }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpNetworkException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="isRetryable">Whether this error is retryable</param>
        /// <param name="context">Additional error context</param>
        public UnsplasharpNetworkException(string message, Exception innerException, string? requestUrl, 
            string? httpMethod, bool isRetryable = true, ErrorContext? context = null)
            : base(message, innerException, requestUrl, httpMethod, context)
        {
            IsRetryable = isRetryable;
        }
    }

    /// <summary>
    /// Exception thrown when a request times out
    /// </summary>
    public class UnsplasharpTimeoutException : UnsplasharpNetworkException
    {
        /// <summary>
        /// The timeout duration
        /// </summary>
        public TimeSpan? Timeout { get; }

        /// <summary>
        /// Initializes a new instance of the UnsplasharpTimeoutException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        /// <param name="timeout">The timeout duration</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="context">Additional error context</param>
        public UnsplasharpTimeoutException(string message, Exception innerException, TimeSpan? timeout, 
            string? requestUrl, string? httpMethod, ErrorContext? context = null)
            : base(message, innerException, requestUrl, httpMethod, true, context)
        {
            Timeout = timeout;
        }
    }
}
