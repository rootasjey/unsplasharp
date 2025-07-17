using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Unsplasharp;
using Unsplasharp.Exceptions;
using UnsplashsharpTest.Data;

namespace UnsplashsharpTest
{
    [TestClass]
    public class ErrorHandlingTests
    {
        private UnsplasharpClient _client = null!;

        [TestInitialize]
        public void Setup()
        {
            _client = new UnsplasharpClient(Credentials.ApplicationId);
        }

        [TestMethod]
        public async Task GetRandomPhoto_BackwardCompatibility_ReturnsNullOnError()
        {
            // Test with invalid application ID to trigger an error
            var invalidClient = new UnsplasharpClient("invalid_app_id");
            
            // The old method should return null instead of throwing
            var photo = await invalidClient.GetRandomPhoto();
            
            Assert.IsNull(photo, "GetRandomPhoto should return null for backward compatibility when an error occurs");
        }

        [TestMethod]
        public async Task GetRandomPhotoAsync_ThrowsException_OnInvalidApplicationId()
        {
            // Test with invalid application ID to trigger an authentication error
            var invalidClient = new UnsplasharpClient("invalid_app_id");
            
            // The new async method should throw an exception
            await Assert.ThrowsExceptionAsync<UnsplasharpAuthenticationException>(
                async () => await invalidClient.GetRandomPhotoAsync(),
                "GetRandomPhotoAsync should throw UnsplasharpAuthenticationException for invalid credentials");
        }

        [TestMethod]
        public async Task GetPhotoAsync_ThrowsNotFoundException_OnInvalidPhotoId()
        {
            // Test with a photo ID that doesn't exist
            var invalidPhotoId = "invalid_photo_id_12345";
            
            var exception = await Assert.ThrowsExceptionAsync<UnsplasharpNotFoundException>(
                async () => await _client.GetPhotoAsync(invalidPhotoId),
                "GetPhotoAsync should throw UnsplasharpNotFoundException for invalid photo ID");

            Assert.AreEqual(invalidPhotoId, exception.ResourceId, "Exception should contain the invalid resource ID");
            Assert.AreEqual("photo", exception.ResourceType, "Exception should indicate the resource type");
            Assert.IsNotNull(exception.Context, "Exception should contain error context");
        }

        [TestMethod]
        public void UnsplasharpException_ContainsProperContext()
        {
            // Create a mock HTTP response for testing
            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.unsplash.com/photos/invalid")
            };

            var exception = UnsplasharpException.FromHttpResponse(response, "Not Found", "test_app_id", "test_correlation");

            Assert.IsInstanceOfType(exception, typeof(UnsplasharpNotFoundException), "Should create NotFoundException for 404 status");
            Assert.IsNotNull(exception.Context, "Exception should contain context");
            Assert.AreEqual("test_app_id", exception.Context.ApplicationId, "Context should contain application ID");
            Assert.AreEqual("test_correlation", exception.Context.CorrelationId, "Context should contain correlation ID");
            Assert.IsNotNull(exception.RequestUrl, "Exception should contain request URL");
            Assert.AreEqual("GET", exception.HttpMethod, "Exception should contain HTTP method");
        }

        [TestMethod]
        public void UnsplasharpHttpException_IdentifiesRetryableErrors()
        {
            // Test server error (retryable)
            var serverErrorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.unsplash.com/photos")
            };

            var serverException = UnsplasharpException.FromHttpResponse(serverErrorResponse, "Server Error", "test_app");
            Assert.IsInstanceOfType(serverException, typeof(UnsplasharpHttpException), "Should create HttpException for server errors");
            Assert.IsTrue(((UnsplasharpHttpException)serverException).IsRetryable, "Server errors should be retryable");

            // Test client error (not retryable)
            var clientErrorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.unsplash.com/photos")
            };

            var clientException = UnsplasharpException.FromHttpResponse(clientErrorResponse, "Bad Request", "test_app");
            Assert.IsInstanceOfType(clientException, typeof(UnsplasharpHttpException), "Should create HttpException for client errors");
            Assert.IsFalse(((UnsplasharpHttpException)clientException).IsRetryable, "Client errors should not be retryable");
        }

        [TestMethod]
        public void UnsplasharpRateLimitException_ExtractsRateLimitInfo()
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.unsplash.com/photos")
            };

            // Add rate limit headers
            response.Headers.Add("X-Ratelimit-Limit", "1000");
            response.Headers.Add("X-Ratelimit-Remaining", "0");
            response.Headers.Add("X-Ratelimit-Reset", "1640995200"); // Unix timestamp

            var exception = UnsplasharpException.FromHttpResponse(response, "Rate limit exceeded", "test_app");
            
            Assert.IsInstanceOfType(exception, typeof(UnsplasharpRateLimitException), "Should create RateLimitException for 429 status");
            
            var rateLimitException = (UnsplasharpRateLimitException)exception;
            Assert.AreEqual(1000, rateLimitException.RateLimit, "Should extract rate limit");
            Assert.AreEqual(0, rateLimitException.RateLimitRemaining, "Should extract remaining requests");
            Assert.IsNotNull(rateLimitException.RateLimitReset, "Should extract reset time");
        }

        [TestMethod]
        public void ErrorContext_CreatesProperSummary()
        {
            var context = new ErrorContext("test_app_id", "test_correlation")
                .WithRetryAttempts(2)
                .WithElapsedTime(TimeSpan.FromMilliseconds(1500));

            var summary = context.ToSummary();

            Assert.IsTrue(summary.Contains("test_correlation"), "Summary should contain correlation ID");
            Assert.IsTrue(summary.Contains("test_app_id"), "Summary should contain application ID");
            Assert.IsTrue(summary.Contains("Retries: 2"), "Summary should contain retry attempts");
            Assert.IsTrue(summary.Contains("Elapsed: 1500ms"), "Summary should contain elapsed time");
        }

        [TestMethod]
        public void RateLimitInfo_ParsesHeadersCorrectly()
        {
            var response = new HttpResponseMessage();
            response.Headers.Add("X-Ratelimit-Limit", "5000");
            response.Headers.Add("X-Ratelimit-Remaining", "4999");
            response.Headers.Add("X-Ratelimit-Reset", "1640995200");

            var rateLimitInfo = RateLimitInfo.FromHeaders(response.Headers);

            Assert.IsNotNull(rateLimitInfo, "Should parse rate limit headers");
            Assert.AreEqual(5000, rateLimitInfo.Limit, "Should parse limit correctly");
            Assert.AreEqual(4999, rateLimitInfo.Remaining, "Should parse remaining correctly");
            Assert.IsFalse(rateLimitInfo.IsExceeded, "Should not be exceeded with remaining requests");

            // Test exceeded scenario
            response.Headers.Clear();
            response.Headers.Add("X-Ratelimit-Remaining", "0");
            
            var exceededInfo = RateLimitInfo.FromHeaders(response.Headers);
            Assert.IsTrue(exceededInfo.IsExceeded, "Should be exceeded with 0 remaining requests");
        }

        [TestMethod]
        public void RateLimitInfo_HandlesInvalidHeaders()
        {
            var response = new HttpResponseMessage();
            response.Headers.Add("X-Ratelimit-Limit", "invalid");
            response.Headers.Add("X-Ratelimit-Remaining", "also_invalid");

            var rateLimitInfo = RateLimitInfo.FromHeaders(response.Headers);

            // Should return null when headers are invalid (no valid rate limit info found)
            Assert.IsNull(rateLimitInfo, "Should return null when no valid rate limit headers are found");
        }

        [TestMethod]
        public void RateLimitInfo_ReturnsNullForMissingHeaders()
        {
            var response = new HttpResponseMessage();
            // No rate limit headers

            var rateLimitInfo = RateLimitInfo.FromHeaders(response.Headers);

            Assert.IsNull(rateLimitInfo, "Should return null when no rate limit headers are present");
        }

        [TestMethod]
        public void ExceptionFactory_CreatesAppropriateExceptions()
        {
            // Test network exception creation
            var httpException = new HttpRequestException("Network error");
            var networkException = UnsplasharpException.FromHttpRequestException(
                httpException, "https://api.unsplash.com/photos", "GET", "test_app");

            Assert.IsInstanceOfType(networkException, typeof(UnsplasharpNetworkException), "Should create NetworkException");
            Assert.IsTrue(networkException.IsRetryable, "Network exceptions should be retryable");

            // Test timeout exception creation
            var timeoutException = new TaskCanceledException("Request timed out");
            var unsplasharpTimeoutException = UnsplasharpException.FromTaskCanceledException(
                timeoutException, TimeSpan.FromSeconds(30), "https://api.unsplash.com/photos", "GET", "test_app");

            Assert.IsInstanceOfType(unsplasharpTimeoutException, typeof(UnsplasharpTimeoutException), "Should create TimeoutException");
            Assert.AreEqual(TimeSpan.FromSeconds(30), unsplasharpTimeoutException.Timeout, "Should preserve timeout duration");
        }

        [TestMethod]
        public void ErrorContext_HandlesPropertyOperations()
        {
            var context = new ErrorContext("test_app");

            // Test property setting and getting
            context.WithProperty("CustomKey", "CustomValue");
            var retrievedValue = context.GetProperty<string>("CustomKey");

            Assert.AreEqual("CustomValue", retrievedValue, "Should store and retrieve custom properties");

            // Test getting non-existent property
            var nonExistent = context.GetProperty<string>("NonExistent");
            Assert.IsNull(nonExistent, "Should return null for non-existent properties");

            // Test getting property with wrong type
            context.WithProperty("IntValue", 42);
            var wrongType = context.GetProperty<string>("IntValue");
            Assert.IsNull(wrongType, "Should return null when property type doesn't match");
        }
    }
}
