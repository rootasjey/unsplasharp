using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Unsplasharp.Exceptions
{
    /// <summary>
    /// Factory class for creating Unsplasharp exceptions with proper context
    /// </summary>
    internal static class ExceptionFactory
    {
        /// <summary>
        /// Creates an appropriate exception from an HTTP response
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="responseContent">The response content</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An appropriate UnsplasharpException</returns>
        public static UnsplasharpException FromHttpResponse(HttpResponseMessage response, string? responseContent, 
            string? applicationId = null, string? correlationId = null)
        {
            var context = ErrorContext.FromResponse(response, applicationId, correlationId);
            var requestUrl = response.RequestMessage?.RequestUri?.ToString();
            var httpMethod = response.RequestMessage?.Method?.Method;

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    return new UnsplasharpAuthenticationException(
                        "Authentication failed. Please check your application ID or access token.",
                        requestUrl, httpMethod, context);

                case HttpStatusCode.NotFound:
                    var (resourceId, resourceType) = ExtractResourceInfo(requestUrl);
                    return new UnsplasharpNotFoundException(
                        $"The requested {resourceType ?? "resource"} was not found.",
                        resourceId, resourceType, requestUrl, httpMethod, context);

                case (HttpStatusCode)429: // TooManyRequests
                    var rateLimitInfo = context.RateLimitInfo;
                    return new UnsplasharpRateLimitException(
                        "Rate limit exceeded. Please wait before making more requests.",
                        rateLimitInfo?.Limit, rateLimitInfo?.Remaining, rateLimitInfo?.Reset,
                        requestUrl, httpMethod, context);

                case HttpStatusCode.BadRequest:
                    return new UnsplasharpHttpException(
                        "Bad request. Please check your request parameters.",
                        response.StatusCode, responseContent, requestUrl, httpMethod, false, context);

                case HttpStatusCode.Forbidden:
                    return new UnsplasharpHttpException(
                        "Access forbidden. You may not have permission to access this resource.",
                        response.StatusCode, responseContent, requestUrl, httpMethod, false, context);

                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.GatewayTimeout:
                    return new UnsplasharpHttpException(
                        "Server error occurred. This error may be temporary.",
                        response.StatusCode, responseContent, requestUrl, httpMethod, true, context);

                default:
                    var isRetryable = IsRetryableStatusCode(response.StatusCode);
                    return new UnsplasharpHttpException(
                        $"HTTP request failed with status code {(int)response.StatusCode} ({response.StatusCode}).",
                        response.StatusCode, responseContent, requestUrl, httpMethod, isRetryable, context);
            }
        }

        /// <summary>
        /// Creates a network exception from an HttpRequestException
        /// </summary>
        /// <param name="exception">The HttpRequestException</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An UnsplasharpNetworkException</returns>
        public static UnsplasharpNetworkException FromHttpRequestException(HttpRequestException exception, 
            string? requestUrl, string? httpMethod, string? applicationId = null, string? correlationId = null)
        {
            var context = new ErrorContext(applicationId, correlationId);
            
            return new UnsplasharpNetworkException(
                "Network error occurred while making the request.",
                exception, requestUrl, httpMethod, true, context);
        }

        /// <summary>
        /// Creates a timeout exception from a TaskCanceledException
        /// </summary>
        /// <param name="exception">The TaskCanceledException</param>
        /// <param name="timeout">The timeout duration</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An UnsplasharpTimeoutException</returns>
        public static UnsplasharpTimeoutException FromTaskCanceledException(TaskCanceledException exception, 
            TimeSpan? timeout, string? requestUrl, string? httpMethod, string? applicationId = null, string? correlationId = null)
        {
            var context = new ErrorContext(applicationId, correlationId);
            
            return new UnsplasharpTimeoutException(
                "Request timed out. The server may be experiencing high load.",
                exception, timeout, requestUrl, httpMethod, context);
        }

        /// <summary>
        /// Creates a parsing exception from a JsonException
        /// </summary>
        /// <param name="exception">The JsonException</param>
        /// <param name="rawContent">The raw content that failed to parse</param>
        /// <param name="expectedType">The expected data type</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An UnsplasharpParsingException</returns>
        public static UnsplasharpParsingException FromJsonException(JsonException exception, string? rawContent, 
            string? expectedType, string? requestUrl, string? httpMethod, string? applicationId = null, string? correlationId = null)
        {
            var context = new ErrorContext(applicationId, correlationId);
            
            return new UnsplasharpParsingException(
                "Failed to parse the API response. The response format may have changed.",
                exception, rawContent, expectedType, requestUrl, httpMethod, context);
        }

        /// <summary>
        /// Creates a generic exception for unexpected errors
        /// </summary>
        /// <param name="exception">The original exception</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An UnsplasharpException</returns>
        public static UnsplasharpException FromGenericException(Exception exception, string? requestUrl, 
            string? httpMethod, string? applicationId = null, string? correlationId = null)
        {
            var context = new ErrorContext(applicationId, correlationId);
            
            return new UnsplasharpNetworkException(
                "An unexpected error occurred while processing the request.",
                exception, requestUrl, httpMethod, false, context);
        }

        /// <summary>
        /// Determines if an HTTP status code indicates a retryable error
        /// </summary>
        /// <param name="statusCode">The HTTP status code</param>
        /// <returns>True if the error is retryable</returns>
        private static bool IsRetryableStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.InternalServerError => true,
                HttpStatusCode.BadGateway => true,
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,
                HttpStatusCode.RequestTimeout => true,
                _ => false
            };
        }

        /// <summary>
        /// Extracts resource information from a request URL
        /// </summary>
        /// <param name="requestUrl">The request URL</param>
        /// <returns>A tuple containing resource ID and type</returns>
        private static (string? resourceId, string? resourceType) ExtractResourceInfo(string? requestUrl)
        {
            if (string.IsNullOrEmpty(requestUrl))
                return (null, null);

            try
            {
                var uri = new Uri(requestUrl);
                var segments = uri.Segments;

                if (segments.Length >= 3)
                {
                    var resourceType = segments[segments.Length - 2].TrimEnd('/'); // Second to last segment
                    var resourceId = segments[segments.Length - 1].TrimEnd('/');   // Last segment

                    // Handle common Unsplash API patterns
                    return resourceType switch
                    {
                        "photos" => (resourceId, "photo"),
                        "collections" => (resourceId, "collection"),
                        "users" => (resourceId, "user"),
                        _ => (resourceId, resourceType)
                    };
                }
            }
            catch
            {
                // If URL parsing fails, return null values
            }

            return (null, null);
        }
    }

    /// <summary>
    /// Base exception class for all Unsplasharp-related errors (partial class for factory methods)
    /// </summary>
    public abstract partial class UnsplasharpException
    {
        /// <summary>
        /// Creates an exception from an HTTP response
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <param name="responseContent">The response content</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An appropriate UnsplasharpException</returns>
        public static UnsplasharpException FromHttpResponse(HttpResponseMessage response, string? responseContent, 
            string? applicationId = null, string? correlationId = null)
        {
            return ExceptionFactory.FromHttpResponse(response, responseContent, applicationId, correlationId);
        }

        /// <summary>
        /// Creates an exception from an HttpRequestException
        /// </summary>
        /// <param name="exception">The HttpRequestException</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An UnsplasharpNetworkException</returns>
        public static UnsplasharpNetworkException FromHttpRequestException(HttpRequestException exception, 
            string? requestUrl, string? httpMethod, string? applicationId = null, string? correlationId = null)
        {
            return ExceptionFactory.FromHttpRequestException(exception, requestUrl, httpMethod, applicationId, correlationId);
        }

        /// <summary>
        /// Creates an exception from a TaskCanceledException
        /// </summary>
        /// <param name="exception">The TaskCanceledException</param>
        /// <param name="timeout">The timeout duration</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An UnsplasharpTimeoutException</returns>
        public static UnsplasharpTimeoutException FromTaskCanceledException(TaskCanceledException exception, 
            TimeSpan? timeout, string? requestUrl, string? httpMethod, string? applicationId = null, string? correlationId = null)
        {
            return ExceptionFactory.FromTaskCanceledException(exception, timeout, requestUrl, httpMethod, applicationId, correlationId);
        }

        /// <summary>
        /// Creates an exception from a JsonException
        /// </summary>
        /// <param name="exception">The JsonException</param>
        /// <param name="rawContent">The raw content that failed to parse</param>
        /// <param name="expectedType">The expected data type</param>
        /// <param name="requestUrl">The request URL</param>
        /// <param name="httpMethod">The HTTP method</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>An UnsplasharpParsingException</returns>
        public static UnsplasharpParsingException FromJsonException(JsonException exception, string? rawContent, 
            string? expectedType, string? requestUrl, string? httpMethod, string? applicationId = null, string? correlationId = null)
        {
            return ExceptionFactory.FromJsonException(exception, rawContent, expectedType, requestUrl, httpMethod, applicationId, correlationId);
        }
    }
}
