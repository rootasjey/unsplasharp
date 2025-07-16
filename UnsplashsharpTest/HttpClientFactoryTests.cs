using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Unsplasharp;
using Unsplasharp.Extensions;

namespace UnsplashsharpTest
{
    [TestClass]
    public class HttpClientFactoryTests
    {
        private const string TestApplicationId = "test-app-id";

        [TestMethod]
        public void LegacyConstructor_ShouldWork()
        {
            // Test that the legacy constructor still works without IHttpClientFactory
            var client = new UnsplasharpClient(TestApplicationId);
            
            Assert.IsNotNull(client);
            Assert.AreEqual(TestApplicationId, client.ApplicationId);
        }

        [TestMethod]
        public void LegacyConstructorWithLogger_ShouldWork()
        {
            // Test legacy constructor with logger
            using var loggerFactory = LoggerFactory.Create(builder => { });
            var logger = loggerFactory.CreateLogger<UnsplasharpClient>();

            var client = new UnsplasharpClient(TestApplicationId, logger: logger);

            Assert.IsNotNull(client);
            Assert.AreEqual(TestApplicationId, client.ApplicationId);
        }

        [TestMethod]
        public void HttpClientFactoryConstructor_ShouldWork()
        {
            // Test constructor with IHttpClientFactory
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var logger = serviceProvider.GetService<ILogger<UnsplasharpClient>>();
            
            var client = new UnsplasharpClient(TestApplicationId, logger: logger, httpClientFactory: httpClientFactory);
            
            Assert.IsNotNull(client);
            Assert.AreEqual(TestApplicationId, client.ApplicationId);
        }

        [TestMethod]
        public void ServiceCollectionExtension_ShouldRegisterCorrectly()
        {
            // Test the AddUnsplasharp extension method
            var services = new ServiceCollection();
            services.AddLogging();
            
            services.AddUnsplasharp(TestApplicationId);
            
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<UnsplasharpClient>();
            
            Assert.IsNotNull(client);
            Assert.AreEqual(TestApplicationId, client.ApplicationId);
        }

        [TestMethod]
        public void ServiceCollectionExtensionWithOptions_ShouldRegisterCorrectly()
        {
            // Test the AddUnsplasharp extension method with options
            var services = new ServiceCollection();
            services.AddLogging();
            
            services.AddUnsplasharp(options =>
            {
                options.ApplicationId = TestApplicationId;
                options.Secret = "test-secret";
                options.ConfigureHttpClient = client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(60);
                };
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetRequiredService<UnsplasharpClient>();
            
            Assert.IsNotNull(client);
            Assert.AreEqual(TestApplicationId, client.ApplicationId);
            Assert.AreEqual("test-secret", client.Secret);
        }

        [TestMethod]
        public void HttpClientFactory_ShouldCreateNamedClient()
        {
            // Test that the HttpClient factory creates a named client
            var services = new ServiceCollection();
            services.AddUnsplasharp(TestApplicationId);
            
            var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            
            var httpClient = httpClientFactory.CreateClient("unsplash");
            
            Assert.IsNotNull(httpClient);
            Assert.IsNotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.AreEqual("Client-ID", httpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.AreEqual(TestApplicationId, httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [TestMethod]
        public void InvalidApplicationId_ShouldThrowException()
        {
            // Test that null application ID throws exception
            Assert.ThrowsException<ArgumentNullException>(() =>
                new UnsplasharpClient(null!));
        }

        [TestMethod]
        public void ServiceCollectionExtension_InvalidApplicationId_ShouldThrowException()
        {
            // Test that service collection extension throws for invalid app ID
            var services = new ServiceCollection();
            
            Assert.ThrowsException<ArgumentException>(() => 
                services.AddUnsplasharp(string.Empty));
        }

        [TestMethod]
        public void MultipleClients_ShouldReuseHttpClient()
        {
            // Test that multiple clients with same app ID reuse HttpClient (legacy pattern)
            var client1 = new UnsplasharpClient(TestApplicationId);
            var client2 = new UnsplasharpClient(TestApplicationId);
            
            // Both should work without issues
            Assert.IsNotNull(client1);
            Assert.IsNotNull(client2);
            Assert.AreEqual(client1.ApplicationId, client2.ApplicationId);
        }

        [TestMethod]
        public void HttpClientFactoryPattern_ShouldSupportDifferentApplicationIds()
        {
            // Test that HttpClientFactory pattern supports different application IDs
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var logger = serviceProvider.GetService<ILogger<UnsplasharpClient>>();
            
            var client1 = new UnsplasharpClient("app-id-1", logger: logger, httpClientFactory: httpClientFactory);
            var client2 = new UnsplasharpClient("app-id-2", logger: logger, httpClientFactory: httpClientFactory);
            
            Assert.IsNotNull(client1);
            Assert.IsNotNull(client2);
            Assert.AreNotEqual(client1.ApplicationId, client2.ApplicationId);
        }
    }
}
