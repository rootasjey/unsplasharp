using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Unsplasharp.Exceptions
{
    /// <summary>
    /// Contains contextual information about an error for debugging and logging purposes
    /// </summary>
    public class ErrorContext
    {
        /// <summary>
        /// The timestamp when the error occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>
        /// The application ID used for the request
        /// </summary>
        public string? ApplicationId { get; }

        /// <summary>
        /// Request headers sent with the HTTP request
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; }

        /// <summary>
        /// Response headers received from the HTTP response
        /// </summary>
        public Dictionary<string, string> ResponseHeaders { get; }

        /// <summary>
        /// The current rate limit information
        /// </summary>
        public RateLimitInfo? RateLimitInfo { get; set; }

        /// <summary>
        /// The number of retry attempts made
        /// </summary>
        public int RetryAttempts { get; }

        /// <summary>
        /// The total time elapsed for the request (including retries)
        /// </summary>
        public TimeSpan? ElapsedTime { get; }

        /// <summary>
        /// Additional custom properties for context
        /// </summary>
        public Dictionary<string, object> Properties { get; }

        /// <summary>
        /// The correlation ID for tracking requests across logs
        /// </summary>
        public string? CorrelationId { get; }

        /// <summary>
        /// Initializes a new instance of the ErrorContext class
        /// </summary>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        public ErrorContext(string? applicationId = null, string? correlationId = null)
        {
            Timestamp = DateTimeOffset.UtcNow;
            ApplicationId = applicationId;
            CorrelationId = correlationId ?? Guid.NewGuid().ToString("N").Substring(0, 8);
            RequestHeaders = new Dictionary<string, string>();
            ResponseHeaders = new Dictionary<string, string>();
            Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates an ErrorContext from an HTTP request message
        /// </summary>
        /// <param name="request">The HTTP request message</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>A new ErrorContext instance</returns>
        public static ErrorContext FromRequest(HttpRequestMessage request, string? applicationId = null, string? correlationId = null)
        {
            var context = new ErrorContext(applicationId, correlationId);

            // Extract request headers
            foreach (var header in request.Headers)
            {
                context.RequestHeaders[header.Key] = string.Join(", ", header.Value);
            }

            if (request.Content?.Headers != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    context.RequestHeaders[header.Key] = string.Join(", ", header.Value);
                }
            }

            return context;
        }

        /// <summary>
        /// Creates an ErrorContext from an HTTP response message
        /// </summary>
        /// <param name="response">The HTTP response message</param>
        /// <param name="applicationId">The application ID</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <returns>A new ErrorContext instance</returns>
        public static ErrorContext FromResponse(HttpResponseMessage response, string? applicationId = null, string? correlationId = null)
        {
            var context = new ErrorContext(applicationId, correlationId);

            // Extract request headers
            if (response.RequestMessage?.Headers != null)
            {
                foreach (var header in response.RequestMessage.Headers)
                {
                    context.RequestHeaders[header.Key] = string.Join(", ", header.Value);
                }

                if (response.RequestMessage.Content?.Headers != null)
                {
                    foreach (var header in response.RequestMessage.Content.Headers)
                    {
                        context.RequestHeaders[header.Key] = string.Join(", ", header.Value);
                    }
                }
            }

            // Extract response headers
            foreach (var header in response.Headers)
            {
                context.ResponseHeaders[header.Key] = string.Join(", ", header.Value);
            }

            if (response.Content?.Headers != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    context.ResponseHeaders[header.Key] = string.Join(", ", header.Value);
                }
            }

            // Extract rate limit information
            context.RateLimitInfo = RateLimitInfo.FromHeaders(response.Headers);

            return context;
        }

        /// <summary>
        /// Adds a custom property to the context
        /// </summary>
        /// <param name="key">The property key</param>
        /// <param name="value">The property value</param>
        /// <returns>This ErrorContext instance for method chaining</returns>
        public ErrorContext WithProperty(string key, object value)
        {
            Properties[key] = value;
            return this;
        }

        /// <summary>
        /// Sets the retry attempts count
        /// </summary>
        /// <param name="attempts">The number of retry attempts</param>
        /// <returns>This ErrorContext instance for method chaining</returns>
        public ErrorContext WithRetryAttempts(int attempts)
        {
            return WithProperty("RetryAttempts", attempts);
        }

        /// <summary>
        /// Sets the elapsed time
        /// </summary>
        /// <param name="elapsed">The elapsed time</param>
        /// <returns>This ErrorContext instance for method chaining</returns>
        public ErrorContext WithElapsedTime(TimeSpan elapsed)
        {
            return WithProperty("ElapsedTime", elapsed);
        }

        /// <summary>
        /// Gets a property value by key
        /// </summary>
        /// <typeparam name="T">The expected type of the property</typeparam>
        /// <param name="key">The property key</param>
        /// <returns>The property value, or default(T) if not found</returns>
        public T? GetProperty<T>(string key)
        {
            if (Properties.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default(T);
        }

        /// <summary>
        /// Creates a summary string of the error context for logging
        /// </summary>
        /// <returns>A formatted summary string</returns>
        public string ToSummary()
        {
            var summary = $"[{CorrelationId}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC";
            
            if (!string.IsNullOrEmpty(ApplicationId))
            {
                summary += $" | App: {ApplicationId}";
            }

            if (RateLimitInfo != null)
            {
                summary += $" | Rate: {RateLimitInfo.Remaining}/{RateLimitInfo.Limit}";
            }

            var retryAttempts = GetProperty<int>("RetryAttempts");
            if (retryAttempts > 0)
            {
                summary += $" | Retries: {retryAttempts}";
            }

            var elapsedTime = GetProperty<TimeSpan?>("ElapsedTime");
            if (elapsedTime.HasValue)
            {
                summary += $" | Elapsed: {elapsedTime.Value.TotalMilliseconds:F0}ms";
            }

            return summary;
        }
    }

    /// <summary>
    /// Contains rate limit information extracted from HTTP headers
    /// </summary>
    public class RateLimitInfo
    {
        /// <summary>
        /// The maximum number of requests allowed per hour
        /// </summary>
        public int? Limit { get; }

        /// <summary>
        /// The number of requests remaining in the current hour
        /// </summary>
        public int? Remaining { get; }

        /// <summary>
        /// When the rate limit resets
        /// </summary>
        public DateTimeOffset? Reset { get; }

        /// <summary>
        /// Initializes a new instance of the RateLimitInfo class
        /// </summary>
        /// <param name="limit">The rate limit</param>
        /// <param name="remaining">The remaining requests</param>
        /// <param name="reset">When the rate limit resets</param>
        public RateLimitInfo(int? limit, int? remaining, DateTimeOffset? reset)
        {
            Limit = limit;
            Remaining = remaining;
            Reset = reset;
        }

        /// <summary>
        /// Extracts rate limit information from HTTP response headers
        /// </summary>
        /// <param name="headers">The HTTP response headers</param>
        /// <returns>A RateLimitInfo instance, or null if no rate limit headers found</returns>
        public static RateLimitInfo? FromHeaders(System.Net.Http.Headers.HttpResponseHeaders headers)
        {
            int? limit = null;
            int? remaining = null;
            DateTimeOffset? reset = null;

            // Try to extract X-Ratelimit-Limit
            if (headers.TryGetValues("X-Ratelimit-Limit", out var limitValues))
            {
                if (int.TryParse(limitValues.FirstOrDefault(), out var limitValue))
                {
                    limit = limitValue;
                }
            }

            // Try to extract X-Ratelimit-Remaining
            if (headers.TryGetValues("X-Ratelimit-Remaining", out var remainingValues))
            {
                if (int.TryParse(remainingValues.FirstOrDefault(), out var remainingValue))
                {
                    remaining = remainingValue;
                }
            }

            // Try to extract X-Ratelimit-Reset (Unix timestamp)
            if (headers.TryGetValues("X-Ratelimit-Reset", out var resetValues))
            {
                if (long.TryParse(resetValues.FirstOrDefault(), out var resetValue))
                {
                    reset = DateTimeOffset.FromUnixTimeSeconds(resetValue);
                }
            }

            // Return null if no rate limit information was found
            if (limit == null && remaining == null && reset == null)
            {
                return null;
            }

            return new RateLimitInfo(limit, remaining, reset);
        }

        /// <summary>
        /// Indicates whether the rate limit has been exceeded
        /// </summary>
        public bool IsExceeded => Remaining.HasValue && Remaining.Value <= 0;

        /// <summary>
        /// Gets the time until the rate limit resets
        /// </summary>
        public TimeSpan? TimeUntilReset => Reset?.Subtract(DateTimeOffset.UtcNow);
    }
}
