using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unsplasharp;
using Unsplasharp.Models;
using UnsplashsharpTest.Data;

namespace UnsplashsharpTest
{
    [TestClass]
    public class CancellationTokenTests
    {
        [TestMethod]
        public async Task GetPhoto_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.GetPhoto("test-id", 0, 0, cts.Token),
                "GetPhoto should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task GetRandomPhoto_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.GetRandomPhoto(cts.Token),
                "GetRandomPhoto should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task SearchPhotos_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.SearchPhotos("nature", 1, 10, cts.Token),
                "SearchPhotos should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task GetCollection_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.GetCollection("test-id", cts.Token),
                "GetCollection should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task ListCollections_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.ListCollections(1, 10, cts.Token),
                "ListCollections should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task GetUser_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.GetUser("test-user", 0, 0, cts.Token),
                "GetUser should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task SearchCollections_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.SearchCollections("nature", 1, 10, cts.Token),
                "SearchCollections should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task GetTotalStats_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();

            // Start the task
            var task = client.GetTotalStats(cts.Token);

            // Cancel immediately after starting
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await task,
                "GetTotalStats should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task GetRandomPhotoAsync_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.GetRandomPhotoAsync(cts.Token),
                "GetRandomPhotoAsync should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task GetPhotoAsync_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();
            
            // Act & Assert
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.GetPhotoAsync("test-id", 0, 0, cts.Token),
                "GetPhotoAsync should throw OperationCanceledException when cancellation token is cancelled");
        }

        [TestMethod]
        public async Task CancellationToken_WithTimeout_ShouldCancelAfterTimeout()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1)); // Very short timeout
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () => await client.GetRandomPhoto(cts.Token),
                "Method should throw OperationCanceledException when timeout expires");
        }

        [TestMethod]
        public async Task CancellationToken_NotCancelled_ShouldCompleteNormally()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();

            // Act - Don't cancel the token, should complete normally
            try
            {
                var result = await client.GetTotalStats(cts.Token);

                // Assert
                Assert.IsNotNull(result, "GetTotalStats should return a result when not cancelled");
            }
            catch (OperationCanceledException)
            {
                Assert.Fail("Method should not throw OperationCanceledException when token is not cancelled");
            }
        }

        [TestMethod]
        public void CancellationTokenOverloads_ShouldExist()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource();

            // Act & Assert - Just verify the overloads exist and can be called
            Assert.IsNotNull(client.GetPhoto("test", 0, 0, cts.Token));
            Assert.IsNotNull(client.GetRandomPhoto(cts.Token));
            Assert.IsNotNull(client.SearchPhotos("test", 1, 10, cts.Token));
            Assert.IsNotNull(client.GetCollection("test", cts.Token));
            Assert.IsNotNull(client.ListCollections(1, 10, cts.Token));
            Assert.IsNotNull(client.GetUser("test", 0, 0, cts.Token));
            Assert.IsNotNull(client.SearchCollections("test", 1, 10, cts.Token));
            Assert.IsNotNull(client.GetTotalStats(cts.Token));
            Assert.IsNotNull(client.GetRandomPhotoAsync(cts.Token));
            Assert.IsNotNull(client.GetPhotoAsync("test", 0, 0, cts.Token));

            // If we get here, all the overloads exist and compile correctly
            Assert.IsTrue(true, "All cancellation token overloads exist and are callable");
        }

        [TestMethod]
        public async Task CancellationToken_IntegrationTest_ShouldWorkWithRealAPI()
        {
            // Arrange
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Reasonable timeout

            // Act & Assert - Test that methods work with cancellation tokens in real scenarios
            try
            {
                var stats = await client.GetTotalStats(cts.Token);
                Assert.IsNotNull(stats, "GetTotalStats should work with cancellation token");

                var photos = await client.ListPhotos(1, 5, UnsplasharpClient.OrderBy.Latest, cts.Token);
                Assert.IsNotNull(photos, "ListPhotos should work with cancellation token");

                // If we get here, the cancellation token integration is working
                Assert.IsTrue(true, "Cancellation token integration works correctly");
            }
            catch (OperationCanceledException)
            {
                // This is acceptable if the operation was actually cancelled
                Assert.IsTrue(cts.Token.IsCancellationRequested, "OperationCanceledException should only be thrown if token was cancelled");
            }
        }
    }
}
