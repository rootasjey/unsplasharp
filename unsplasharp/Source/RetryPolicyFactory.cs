using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Unsplasharp.Exceptions;

namespace Unsplasharp
{
    /// <summary>
    /// Factory for creating retry policies with intelligent error handling
    /// </summary>
    internal static class RetryPolicyFactory
    {
        /// <summary>
        /// Creates a comprehensive retry policy for HTTP requests
        /// </summary>
        /// <param name="logger">Logger for retry events</param>
        /// <param name="applicationId">Application ID for context</param>
        /// <returns>A configured ResiliencePipeline</returns>
        public static ResiliencePipeline CreateRetryPolicy(ILogger logger, string applicationId)
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(new Polly.Retry.RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>(),
                    
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = Polly.DelayBackoffType.Exponential,
                    UseJitter = true, // Add jitter to prevent thundering herd
                    
                    OnRetry = args =>
                    {
                        var attemptNumber = args.AttemptNumber + 1;
                        var maxAttempts = 3;
                        
                        if (args.Outcome.Exception != null)
                        {
                            logger.LogWarning("Retrying HTTP request due to exception (attempt {AttemptNumber}/{MaxAttempts}). " +
                                             "Exception: {ExceptionType} - {ExceptionMessage}",
                                attemptNumber, maxAttempts, 
                                args.Outcome.Exception.GetType().Name, 
                                args.Outcome.Exception.Message);
                        }
                        else if (args.Outcome.Result is HttpResponseMessage response)
                        {
                            logger.LogWarning("Retrying HTTP request due to status code {StatusCode} (attempt {AttemptNumber}/{MaxAttempts}). " +
                                             "URL: {RequestUrl}",
                                (int)response.StatusCode, attemptNumber, maxAttempts,
                                response.RequestMessage?.RequestUri?.ToString());
                        }
                        
                        return default;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(30)) // Add timeout policy
                .Build();
        }

        /// <summary>
        /// Creates a retry policy specifically for rate-limited requests
        /// </summary>
        /// <param name="logger">Logger for retry events</param>
        /// <param name="applicationId">Application ID for context</param>
        /// <returns>A configured ResiliencePipeline for rate-limited scenarios</returns>
        public static ResiliencePipeline CreateRateLimitRetryPolicy(ILogger logger, string applicationId)
        {
            return new ResiliencePipelineBuilder()
                .AddRetry(new Polly.Retry.RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>(),
                    
                    MaxRetryAttempts = 2, // Fewer retries for rate limits
                    DelayGenerator = args =>
                    {
                        // Use rate limit reset time if available
                        if (args.Outcome.Result is HttpResponseMessage response)
                        {
                            var rateLimitInfo = RateLimitInfo.FromHeaders(response.Headers);
                            if (rateLimitInfo?.TimeUntilReset.HasValue == true)
                            {
                                var resetTime = rateLimitInfo.TimeUntilReset.Value;
                                // Add a small buffer and cap at 5 minutes
                                var delay = TimeSpan.FromSeconds(Math.Min(resetTime.TotalSeconds + 10, 300));
                                return new ValueTask<TimeSpan?>(delay);
                            }
                        }
                        
                        // Fallback to exponential backoff
                        var baseDelay = TimeSpan.FromSeconds(60); // Start with 1 minute
                        var exponentialDelay = TimeSpan.FromMilliseconds(
                            baseDelay.TotalMilliseconds * Math.Pow(2, args.AttemptNumber));
                        return new ValueTask<TimeSpan?>(exponentialDelay);
                    },
                    
                    OnRetry = args =>
                    {
                        var attemptNumber = args.AttemptNumber + 1;
                        var maxAttempts = 2;
                        
                        if (args.Outcome.Result is HttpResponseMessage response)
                        {
                            var rateLimitInfo = RateLimitInfo.FromHeaders(response.Headers);
                            logger.LogWarning("Rate limit exceeded, retrying (attempt {AttemptNumber}/{MaxAttempts}). " +
                                             "Rate limit: {Remaining}/{Limit}, Reset: {Reset}",
                                attemptNumber, maxAttempts,
                                rateLimitInfo?.Remaining, rateLimitInfo?.Limit, rateLimitInfo?.Reset);
                        }
                        
                        return default;
                    }
                })
                .Build();
        }

        /// <summary>
        /// Determines if an HTTP response should trigger a retry
        /// </summary>
        /// <param name="response">The HTTP response</param>
        /// <returns>True if the request should be retried</returns>
        private static bool ShouldRetryResponse(HttpResponseMessage response)
        {
            // Retry on server errors (5xx)
            if ((int)response.StatusCode >= 500)
                return true;

            // Retry on specific client errors that might be transient
            return response.StatusCode switch
            {
                HttpStatusCode.RequestTimeout => true,
                (HttpStatusCode)429 => true, // TooManyRequests - Handle separately with rate limit policy
                _ => false
            };
        }

        /// <summary>
        /// Creates a circuit breaker policy for protecting against cascading failures
        /// </summary>
        /// <param name="logger">Logger for circuit breaker events</param>
        /// <param name="applicationId">Application ID for context</param>
        /// <returns>A configured ResiliencePipeline with circuit breaker</returns>
        public static ResiliencePipeline CreateCircuitBreakerPolicy(ILogger logger, string applicationId)
        {
            return new ResiliencePipelineBuilder()
                .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>(),

                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 3,
                    BreakDuration = TimeSpan.FromSeconds(30),

                    OnOpened = args =>
                    {
                        logger.LogWarning("Circuit breaker opened for application {ApplicationId}. " +
                                         "Too many failures detected.", applicationId);
                        return default;
                    },

                    OnClosed = args =>
                    {
                        logger.LogInformation("Circuit breaker closed for application {ApplicationId}. " +
                                            "Service appears to be healthy again.", applicationId);
                        return default;
                    },

                    OnHalfOpened = args =>
                    {
                        logger.LogInformation("Circuit breaker half-opened for application {ApplicationId}. " +
                                            "Testing service health.", applicationId);
                        return default;
                    }
                })
                .Build();
        }

        /// <summary>
        /// Creates a comprehensive resilience pipeline combining multiple policies
        /// </summary>
        /// <param name="logger">Logger for policy events</param>
        /// <param name="applicationId">Application ID for context</param>
        /// <param name="enableCircuitBreaker">Whether to enable circuit breaker</param>
        /// <returns>A configured ResiliencePipeline</returns>
        public static ResiliencePipeline CreateComprehensivePolicy(ILogger logger, string applicationId, 
            bool enableCircuitBreaker = false)
        {
            var builder = new ResiliencePipelineBuilder();

            // Add timeout first (innermost)
            builder.AddTimeout(TimeSpan.FromSeconds(30));

            // Add retry policy
            builder.AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(),
                
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = Polly.DelayBackoffType.Exponential,
                UseJitter = true,
                
                OnRetry = args =>
                {
                    var attemptNumber = args.AttemptNumber + 1;
                    var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                    
                    if (args.Outcome.Exception != null)
                    {
                        logger.LogWarning("[{CorrelationId}] Retrying HTTP request due to exception " +
                                         "(attempt {AttemptNumber}/3). Exception: {ExceptionType} - {ExceptionMessage}",
                            correlationId, attemptNumber, 
                            args.Outcome.Exception.GetType().Name, 
                            args.Outcome.Exception.Message);
                    }
                    else if (args.Outcome.Result is HttpResponseMessage response)
                    {
                        logger.LogWarning("[{CorrelationId}] Retrying HTTP request due to status code {StatusCode} " +
                                         "(attempt {AttemptNumber}/3). URL: {RequestUrl}",
                            correlationId, (int)response.StatusCode, attemptNumber,
                            response.RequestMessage?.RequestUri?.ToString());
                    }
                    
                    return default;
                }
            });

            // Add circuit breaker if enabled (outermost)
            if (enableCircuitBreaker)
            {
                builder.AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TaskCanceledException>(),

                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromMinutes(1),
                    
                    OnOpened = args =>
                    {
                        logger.LogWarning("Circuit breaker opened for application {ApplicationId}. " +
                                         "Service will be unavailable for {BreakDuration}.", 
                                         applicationId, TimeSpan.FromMinutes(1));
                        return default;
                    }
                });
            }

            return builder.Build();
        }
    }
}
