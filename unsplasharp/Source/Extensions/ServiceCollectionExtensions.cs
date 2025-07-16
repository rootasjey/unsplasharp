using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Unsplasharp.Extensions
{
    /// <summary>
    /// Extension methods for configuring Unsplasharp with IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Unsplasharp client to the service collection with IHttpClientFactory support
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="applicationId">Unsplash application ID</param>
        /// <param name="secret">Optional secret for authenticated requests</param>
        /// <param name="configureHttpClient">Optional action to configure the HttpClient</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddUnsplasharp(
            this IServiceCollection services,
            string applicationId,
            string? secret = null,
            Action<HttpClient>? configureHttpClient = null)
        {
            if (string.IsNullOrEmpty(applicationId))
                throw new ArgumentException("Application ID cannot be null or empty", nameof(applicationId));

            // Configure named HttpClient for Unsplash
            services.AddHttpClient("unsplash", client =>
            {
                // Set default headers
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Client-ID", applicationId);
                
                client.DefaultRequestHeaders.Add("User-Agent", "Unsplasharp/2.0");
                
                // Set reasonable timeout
                client.Timeout = TimeSpan.FromSeconds(30);
                
                // Allow custom configuration
                configureHttpClient?.Invoke(client);
            });

            // Register UnsplasharpClient
            services.AddScoped<UnsplasharpClient>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var logger = provider.GetService<ILogger<UnsplasharpClient>>();
                
                return new UnsplasharpClient(applicationId, secret, logger, httpClientFactory);
            });

            return services;
        }

        /// <summary>
        /// Adds Unsplasharp client to the service collection with custom configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Configuration action</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddUnsplasharp(
            this IServiceCollection services,
            Action<UnsplasharpOptions> configure)
        {
            var options = new UnsplasharpOptions();
            configure(options);

            return services.AddUnsplasharp(
                options.ApplicationId,
                options.Secret,
                options.ConfigureHttpClient);
        }


    }

    /// <summary>
    /// Configuration options for Unsplasharp
    /// </summary>
    public class UnsplasharpOptions
    {
        /// <summary>
        /// Unsplash application ID (required)
        /// </summary>
        public string ApplicationId { get; set; } = string.Empty;

        /// <summary>
        /// Optional secret for authenticated requests
        /// </summary>
        public string? Secret { get; set; }

        /// <summary>
        /// Optional action to configure the HttpClient
        /// </summary>
        public Action<HttpClient>? ConfigureHttpClient { get; set; }
    }
}
