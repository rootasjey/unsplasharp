using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Unsplasharp.Models;
using Unsplasharp.Exceptions;
using System.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Http;
using Polly;

namespace Unsplasharp {
    /// <summary>
    /// Represents a Unsplasharp class which can be used to communicate with Unplash APIs.
    /// </summary>
    public class UnsplasharpClient {
        #region variables
        /// <summary>
        /// API Key
        /// </summary>
        public string? Secret { get; set; }

        /// <summary>
        /// Identify the application making API calls
        /// Must be created through Unplash developer page
        /// https://unsplash.com/developers
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Unplash base URI: "https://api.unsplash.com/"
        /// </summary>
        private static string URIBase {
            get {
                return "https://api.unsplash.com/";
            }
        }

        /// <summary>
        /// Keep a single http client per application id, to avoid https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        /// </summary>
        private static readonly ConcurrentDictionary<string, Lazy<HttpClient>> httpClients = new ConcurrentDictionary<string, Lazy<HttpClient>>();

        /// <summary>
        /// Logger instance for structured logging
        /// </summary>
        private readonly ILogger<UnsplasharpClient> _logger;

        /// <summary>
        /// Retry policy for HTTP requests
        /// </summary>
        private readonly ResiliencePipeline _retryPolicy;

        /// <summary>
        /// Optional IHttpClientFactory for modern HTTP client management
        /// </summary>
        private readonly IHttpClientFactory? _httpClientFactory;

        /// <summary>
        /// Unplash endpoints
        /// </summary>
        private static IDictionary<string, string> URIEndpoints = new Dictionary<string, string>() {
            {"photos", "photos" },
            {"search", "search" },
            {"users", "users" },
            {"collections", "collections" },
            {"search_photos", "search/photos" },
            {"search_collections", "search/collections" },
            {"search_users", "search/users" },
            {"total_stats", "stats/total" },
            {"monthly_stats", "stats/month" }
        };

        /// <summary>
        /// Maximum number of request allowed per hour.
        /// </summary>
        public int MaxRateLimit { get; set; }

        /// <summary>
        /// Remaining allowed requests in the current hour.
        /// </summary>
        public int RateLimitRemaining { get; set; }

        // ------
        // PHOTOS SEARCH
        // ------

        /// <summary>
        /// Last photo search query
        /// </summary>
        public string? LastPhotosSearchQuery { get; set; }

        /// <summary>
        /// Total pages of last search photos query
        /// </summary>
        public int LastPhotosSearchTotalPages { get; set; }

        /// <summary>
        /// Total results of last search photos query
        /// </summary>
        public int LastPhotosSearchTotalResults { get; set; }

        // -----------
        // COLLECTIONS SEARCH
        // -----------

        /// <summary>
        /// Last collection search query
        /// </summary>
        public string? LastCollectionsSearchQuery { get; set; }

        /// <summary>
        /// Total pages of last search collections query
        /// </summary>
        public int LastCollectionsSearchTotalPages { get; set; }

        /// <summary>
        /// Total results of last search collections query
        /// </summary>
        public int LastCollectionsSearchTotalResults { get; set; }

        // -----
        // USERS SEARCH
        // -----

        /// <summary>
        /// Last collection search query
        /// </summary>
        public string? LastUsersSearchQuery { get; set; }

        /// <summary>
        /// Total pages of last search collections query
        /// </summary>
        public int LastUsersSearchTotalPages { get; set; }

        /// <summary>
        /// Total results of last search collections query
        /// </summary>
        public int LastUsersSearchTotalResults { get; set; }

        /// <summary>
        /// How to sort the data.
        /// </summary>
        public enum OrderBy {
            /// <summary>
            /// Sort the data from the most recent to the oldest.
            /// </summary>
            Latest,

            /// <summary>
            /// Sort the data from the the oldest to the most recent.
            /// </summary>
            Oldest,

            /// <summary>
            /// Sort the data from the most popular to the least.
            /// </summary>
            Popular
        }

        /// <summary>
        /// Photo orientation.
        /// </summary>
        public enum Orientation {
            /// <summary>
            /// Landscape orientation.
            /// </summary>
            Landscape,

            /// <summary>
            /// Portrait orientation.
            /// </summary>
            Portrait,

            /// <summary>
            /// Squarish orientation.
            /// </summary>
            Squarish
        }

        /// <summary>
        /// The frequency of the stats.
        /// </summary>
        public enum Resolution {
            /// <summary>
            /// Stats for each day.
            /// </summary>
            Days
        }

        #endregion variables

        /// <summary>
        /// Initialize a new instance of Unplasharp class.
        /// </summary>
        /// <param name="applicationId">A string to identify the current application performing a request.</param>
        /// <param name="secret">A string representing an API secret key to make HTTP authentified calls to Unplash services.</param>
        /// <param name="logger">Optional logger for structured logging.</param>
        /// <param name="httpClientFactory">Optional IHttpClientFactory for modern HTTP client management. If not provided, falls back to legacy HttpClient management.</param>
        public UnsplasharpClient(string applicationId, string? secret = null, ILogger<UnsplasharpClient>? logger = null, IHttpClientFactory? httpClientFactory = null) {
            ApplicationId = applicationId ?? throw new ArgumentNullException(nameof(applicationId));
            Secret = secret;
            _logger = logger ?? NullLogger<UnsplasharpClient>.Instance;
            _httpClientFactory = httpClientFactory;

            // Initialize retry policy using the factory
            _retryPolicy = RetryPolicyFactory.CreateComprehensivePolicy(_logger, ApplicationId);
        }


        // --------------
        // PUBLIC METHODS
        // --------------

        #region photos

        /// <summary>
        /// Returns the photo corresponding to the given id.
        /// </summary>
        /// <param name="id">Photo's unique id to find.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo?> GetPhoto(string id, int width = 0, int height = 0) {
            return await GetPhoto(id, width, height, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the photo corresponding to the given id.
        /// </summary>
        /// <param name="id">Photo's unique id to find.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo?> GetPhoto(string id, int width, int height, CancellationToken cancellationToken) {
            var url = string.Format("{0}/{1}", GetUrl("photos"), id);

            if (width != 0) { url = AddQueryString(url, "w", width); }
            if (height != 0) { url = AddQueryString(url, "h", height); }

            return await FetchPhoto(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the photo corresponding to the given id,
        /// in the specified width and height and cropped according to a specified rectangle.
        /// </summary>
        /// <param name="id">Photo's unique id to find.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <param name="rectX">X origin point of the cropped rectangle.</param>
        /// <param name="rectY">Y origin point of the cropped rectangle.</param>
        /// <param name="rectWidth">Width of the cropped rectangle.</param>
        /// <param name="rectHeight">Height of the cropped rectangle.</param>
        /// <returns>A new Photo class instance which has a custom URL.</returns>
        public async Task<Photo?> GetPhoto(string id, int width, int height, int rectX, int rectY, int rectWidth, int rectHeight) {
            return await GetPhoto(id, width, height, rectX, rectY, rectWidth, rectHeight, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the photo corresponding to the given id,
        /// in the specified width and height and cropped according to a specified rectangle.
        /// </summary>
        /// <param name="id">Photo's unique id to find.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <param name="rectX">X origin point of the cropped rectangle.</param>
        /// <param name="rectY">Y origin point of the cropped rectangle.</param>
        /// <param name="rectWidth">Width of the cropped rectangle.</param>
        /// <param name="rectHeight">Height of the cropped rectangle.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A new Photo class instance which has a custom URL.</returns>
        public async Task<Photo?> GetPhoto(string id, int width, int height, int rectX, int rectY, int rectWidth, int rectHeight, CancellationToken cancellationToken) {
            var url = string.Format("{0}/{1}?w={2}&h={3}&rect={4},{5},{6},{7}",
                                    GetUrl("photos"), id, width, height, rectX, rectY, rectWidth, rectHeight);

            return await FetchPhoto(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a single random photo.
        /// </summary>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo?> GetRandomPhoto() {
            return await GetRandomPhoto(CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a single random photo.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo?> GetRandomPhoto(CancellationToken cancellationToken) {
            _logger.LogInformation("Fetching random photo");
            var url = string.Format("{0}/random", GetUrl("photos"));
            var photo = await FetchPhoto(url, cancellationToken).ConfigureAwait(false);

            if (photo != null) {
                _logger.LogInformation("Successfully retrieved random photo with ID {PhotoId}", photo.Id);
            } else {
                _logger.LogWarning("Failed to retrieve random photo");
            }

            return photo;
        }

        /// <summary>
        /// Retrieve a single random photo from a specific collection.
        /// </summary>
        /// <param name="collectionId">Public collection ID to filter selection.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo?> GetRandomPhoto(string collectionId) {
            return await GetRandomPhoto(collectionId, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a single random photo from a specific collection.
        /// </summary>
        /// <param name="collectionId">Public collection ID to filter selection.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo?> GetRandomPhoto(string collectionId, CancellationToken cancellationToken) {
            var url = string.Format("{0}/random?collections={1}", GetUrl("photos"), collectionId);
            return await FetchPhoto(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a single random photo from specific collections.
        /// </summary>
        /// <param name="collectionIds">Public collection ID('s) to filter selection.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo?> GetRandomPhoto(string[] collectionIds) {
            return await GetRandomPhoto(collectionIds, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a single random photo from specific collections.
        /// </summary>
        /// <param name="collectionIds">Public collection ID(‘s) to filter selection.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo?> GetRandomPhoto(string[] collectionIds, CancellationToken cancellationToken) {
            var url = string.Format("{0}/random?collections={1}",
                GetUrl("photos"), string.Join(",", collectionIds));

            return await FetchPhoto(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a single random photo, given optional filters.
        /// </summary>
        /// <param name="count">The number of photos to return. (Default: 1; max: 30)</param>
        /// <param name="username">Limit selection to a single user.</param>
        /// <param name="query">Limit selection to photos matching a search term.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <returns>A list of random photos.</returns>
        public async Task<List<Photo>> GetRandomPhoto(int count, string username = "", 
            string query = "", int width = 0, int height = 0) {

            var url = string.Format("{0}/random?count={1}", 
                                    GetUrl("photos"), count);

            if (StringNotNull(username)) { url += string.Format("&username={0}", username); }
            if (StringNotNull(query)) { url += string.Format("&query={0}", query); }
            if (width != 0) { url += string.Format("&w={0}", width); }
            if (height != 0) { url += string.Format("&h={0}", height); }

            return await FetchPhotosList(url);
        }

        /// <summary>
        /// Retrieve a single random photo, given optional filters.
        /// </summary>
        /// <param name="orientation">Filter search results by photo orientation.</param>
        /// <param name="username">Limit selection to a single user.</param>
        /// <param name="query">Limit selection to photos matching a search term.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="count">The number of photos to return. (Default: 1; max: 30)</param>
        /// <returns>A list of random photos.</returns>
        public async Task<List<Photo>> GetRandomPhoto(Orientation orientation,
            string username = "", string query = "", int width = 0, int height = 0, int count = 1) {

            var _orientation = ConvertOrientation(orientation);

            var url = string.Format("{0}/random?count={1}&orientation={2}",
                                    GetUrl("photos"), count, _orientation);

            if (StringNotNull(username)) { url += string.Format("&username={0}", username); }
            if (StringNotNull(query)) { url += string.Format("&query={0}", query); }
            if (width != 0) { url += string.Format("&w={0}", width); }
            if (height != 0) { url += string.Format("&h={0}", height); }

            return await FetchPhotosList(url).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a single random photo, given optional filters.
        /// </summary>
        /// <param name="featured">Limit selection to featured photos.</param>
        /// <param name="username">Limit selection to a single user.</param>
        /// <param name="query">Limit selection to photos matching a search term.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="count">The number of photos to return. (Default: 1; max: 30)</param>
        /// <returns>A list of random photos.</returns>
        public async Task<List<Photo>> GetRandomPhoto(bool featured,
            string username = "", string query = "", int width = 0, int height = 0, int count = 1) {

            var url = string.Format("{0}/random?count={1}&featured={2}",
                                    GetUrl("photos"), count, featured);

            if (StringNotNull(username)) { url += string.Format("&username={0}", username); }
            if (StringNotNull(query)) { url += string.Format("&query={0}", query); }
            if (width != 0) { url += string.Format("&w={0}", width); }
            if (height != 0) { url += string.Format("&h={0}", height); }

            return await FetchPhotosList(url);
        }

        /// <summary>
        /// Retrieve a single random photo, given optional filters.
        /// </summary>
        /// <param name="orientation">Filter search results by photo orientation.</param>
        /// <param name="featured">Limit selection to featured photos.</param>
        /// <param name="username">Limit selection to a single user.</param>
        /// <param name="query">Limit selection to photos matching a search term.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="count">The number of photos to return. (Default: 1; max: 30)</param>
        /// <returns>A list of random photos.</returns>
        public async Task<List<Photo>> GetRandomPhoto(Orientation orientation, bool featured,
            string username = "", string query = "", int width = 0, int height = 0, int count = 1) {

            var _orientation = ConvertOrientation(orientation);

            var url = string.Format("{0}/random?orientation={1}&featured={2}&count={3}", 
                                    GetUrl("photos"), _orientation, featured, count);

            if (StringNotNull(username)) { url += string.Format("&username={0}", username); }
            if (StringNotNull(query)) { url += string.Format("&query={0}", query); }
            if (width != 0) { url += string.Format("&w={0}", width); }
            if (height != 0) { url += string.Format("&h={0}", height); }

            return await FetchPhotosList(url).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve total number of downloads,
        /// views and likes of a single photo, 
        /// as well as the historical breakdown 
        /// of these stats in a specific timeframe (default is 30 days).
        /// </summary>
        /// <param name="id">The public id of the photo.</param>
        /// <param name="resolution">The frequency of the stats.</param>
        /// <param name="quantity">The amount of for each stat.</param>
        /// <returns></returns>
        public async Task<PhotoStats?> GetPhotoStats(string id, Resolution resolution = Resolution.Days, int quantity = 30) {
            var url = string.Format(
                "{0}/{1}/statistics?resolution={2}&quantity={3}",
                GetUrl("photos"), id, ConvertResolution(resolution), quantity);

            try {
                string? responseBodyAsText = await Fetch(url).ConfigureAwait(false);
                if (responseBodyAsText == null) return null;

                using var document = JsonDocument.Parse(responseBodyAsText);
                var data = document.RootElement;

                return new PhotoStats() {
                    Id = data.GetString("id") ?? string.Empty,
                    Downloads = ExtractStatsData(data, "downloads") ?? new StatsData(),
                    Views = ExtractStatsData(data, "views") ?? new StatsData(),
                    Likes = ExtractStatsData(data, "likes") ?? new StatsData()
                };

            } catch {
                return null;
            }
        }

        /// <summary>
        /// Retrieve a single photo’s download link.
        /// Preferably hit this endpoint if a photo is downloaded in your application for use
        /// (example: to be displayed on a blog article, to be shared on social media, to be remixed, etc.).
        /// </summary>
        /// <param name="id">The photo’s ID.</param>
        /// <returns>The photo's download link</returns>
        public async Task<string?> GetPhotoDownloadLink(string id) {
            var url = string.Format("{0}/{1}/download", GetUrl("photos"), id);
            var response = await Fetch(url);

            if (response == null) return null;

            using var document = JsonDocument.Parse(response);
            return document.RootElement.GetString("url");
        }

        /// <summary>
        /// Returns a list of all  photos.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="orderBy">How to sort the photos.</param>
        /// <returns>List of all  photos.</returns>
        public async Task<List<Photo>> ListPhotos(int page = 1, int perPage = 10, OrderBy orderBy = OrderBy.Latest) {
            return await ListPhotos(page, perPage, orderBy, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a list of all  photos.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="orderBy">How to sort the photos.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>List of all  photos.</returns>
        public async Task<List<Photo>> ListPhotos(int page, int perPage, OrderBy orderBy, CancellationToken cancellationToken) {
            var url = string.Format(
                "{0}?page={1}&per_page={2}&order_by={3}",
                GetUrl("photos"), page, perPage, ConvertOderBy(orderBy));

            return await FetchPhotosList(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Return a list of photos from the specified URL.
        /// </summary>
        /// <param name="url">API endpoint to fetch the photos' list.</param>
        /// <returns>A list of photos.</returns>
        public async Task<List<Photo>> FetchPhotosList(string url) {
            return await FetchPhotosList(url, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Return a list of photos from the specified URL.
        /// </summary>
        /// <param name="url">API endpoint to fetch the photos' list.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of photos.</returns>
        public async Task<List<Photo>> FetchPhotosList(string url, CancellationToken cancellationToken) {
            var listPhotos = new List<Photo>();

            try {
                var responseBodyAsText = await Fetch(url, cancellationToken).ConfigureAwait(false);
                if (responseBodyAsText == null) return listPhotos;

                using var document = JsonDocument.Parse(responseBodyAsText);

                if (document.RootElement.ValueKind == JsonValueKind.Array) {
                    foreach (var photoElement in document.RootElement.EnumerateArray()) {
                        var photo = ExtractPhoto(photoElement);
                        if (photo != null) {
                            listPhotos.Add(photo);
                        }
                    }
                }

                return listPhotos;

            } catch /*(HttpRequestException hre)*/ {
                return listPhotos;
            }
        }

        /// <summary>
        /// Fetch a photo from an Unplash specified URL
        /// </summary>
        /// <param name="url">URL to fetch photo from.</param>
        /// <returns>A single photo instance.</returns>
        public async Task<Photo?> FetchPhoto(string url) {
            return await FetchPhoto(url, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch a photo from an Unplash specified URL
        /// </summary>
        /// <param name="url">URL to fetch photo from.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A single photo instance.</returns>
        public async Task<Photo?> FetchPhoto(string url, CancellationToken cancellationToken) {
            var response = await Fetch(url, cancellationToken).ConfigureAwait(false);

            if (response == null) return null;

            using var document = JsonDocument.Parse(response);
            var photo = ExtractPhoto(document.RootElement);
            return photo;
        }

        // ---------------
        // EXCEPTION-THROWING VARIANTS
        // ---------------

        /// <summary>
        /// Retrieve a single random photo with comprehensive error handling.
        /// This method throws specific exceptions for different error scenarios.
        /// </summary>
        /// <returns>A new Photo class instance.</returns>
        /// <exception cref="UnsplasharpException">Thrown when the request fails</exception>
        public async Task<Photo> GetRandomPhotoAsync() {
            return await GetRandomPhotoAsync(CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a single random photo with comprehensive error handling.
        /// This method throws specific exceptions for different error scenarios.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A new Photo class instance.</returns>
        /// <exception cref="UnsplasharpException">Thrown when the request fails</exception>
        public async Task<Photo> GetRandomPhotoAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Fetching random photo with exception handling");
            var url = string.Format("{0}/random", GetUrl("photos"));
            var photo = await FetchPhotoWithExceptions(url, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Successfully retrieved random photo with ID {PhotoId}", photo.Id);
            return photo;
        }

        /// <summary>
        /// Returns the photo corresponding to the given id with comprehensive error handling.
        /// This method throws specific exceptions for different error scenarios.
        /// </summary>
        /// <param name="id">Photo's unique id to find.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <returns>A new Photo class instance.</returns>
        /// <exception cref="UnsplasharpException">Thrown when the request fails</exception>
        /// <exception cref="UnsplasharpNotFoundException">Thrown when the photo is not found</exception>
        public async Task<Photo> GetPhotoAsync(string id, int width = 0, int height = 0) {
            return await GetPhotoAsync(id, width, height, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the photo corresponding to the given id with comprehensive error handling.
        /// This method throws specific exceptions for different error scenarios.
        /// </summary>
        /// <param name="id">Photo's unique id to find.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A new Photo class instance.</returns>
        /// <exception cref="UnsplasharpException">Thrown when the request fails</exception>
        /// <exception cref="UnsplasharpNotFoundException">Thrown when the photo is not found</exception>
        public async Task<Photo> GetPhotoAsync(string id, int width, int height, CancellationToken cancellationToken) {
            var url = string.Format("{0}/{1}", GetUrl("photos"), id);

            if (width != 0) { url = AddQueryString(url, "w", width); }
            if (height != 0) { url = AddQueryString(url, "h", height); }

            return await FetchPhotoWithExceptions(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch a photo from an Unsplash specified URL with comprehensive error handling.
        /// This method throws specific exceptions for different error scenarios.
        /// </summary>
        /// <param name="url">URL to fetch photo from.</param>
        /// <returns>A single photo instance.</returns>
        /// <exception cref="UnsplasharpException">Thrown when the request fails</exception>
        /// <exception cref="UnsplasharpParsingException">Thrown when JSON parsing fails</exception>
        private async Task<Photo> FetchPhotoWithExceptions(string url) {
            return await FetchPhotoWithExceptions(url, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch a photo from an Unsplash specified URL with comprehensive error handling.
        /// This method throws specific exceptions for different error scenarios.
        /// </summary>
        /// <param name="url">URL to fetch photo from.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A single photo instance.</returns>
        /// <exception cref="UnsplasharpException">Thrown when the request fails</exception>
        /// <exception cref="UnsplasharpParsingException">Thrown when JSON parsing fails</exception>
        private async Task<Photo> FetchPhotoWithExceptions(string url, CancellationToken cancellationToken) {
            var response = await FetchWithExceptions(url, cancellationToken).ConfigureAwait(false);

            try {
                using var document = JsonDocument.Parse(response);
                var photo = ExtractPhoto(document.RootElement);

                if (photo == null) {
                    var context = new ErrorContext(ApplicationId);
                    throw new UnsplasharpParsingException(
                        "Failed to extract photo data from API response",
                        new InvalidOperationException("ExtractPhoto returned null"),
                        response, "Photo", url, "GET", context);
                }

                return photo;
            } catch (JsonException ex) {
                throw UnsplasharpException.FromJsonException(ex, response, "Photo", url, "GET", ApplicationId);
            }
        }

        #endregion photos

        #region collections
        /// <summary>
        /// Returns a list of collections (of photos) from a specified URL
        /// </summary>
        /// <param name="url">API endpoint where to fetch the photos' list.</param>
        /// <returns>Collections list</returns>
        public async Task<List<Collection>> FetchCollectionsList(string url) {
            return await FetchCollectionsList(url, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a list of collections (of photos) from a specified URL
        /// </summary>
        /// <param name="url">API endpoint where to fetch the photos' list.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>Collections list</returns>
        public async Task<List<Collection>> FetchCollectionsList(string url, CancellationToken cancellationToken) {
            var listCollection = new List<Collection>();

            try {
                var responseBodyAsText = await Fetch(url, cancellationToken).ConfigureAwait(false);
                if (responseBodyAsText == null) return listCollection;

                using var document = JsonDocument.Parse(responseBodyAsText);

                if (document.RootElement.ValueKind == JsonValueKind.Array) {
                    foreach (var collectionElement in document.RootElement.EnumerateArray()) {
                        var collection = ExtractCollection(collectionElement);
                        if (collection != null) {
                            listCollection.Add(collection);
                        }
                    }
                }

                return listCollection;

            } catch /*(HttpRequestException hre)*/ {
                return listCollection;
            }
        }

        /// <summary>
        /// Returns the photos collection corresponding to the given id.
        /// </summary>
        /// <param name="id">Collection's id to find.</param>
        /// <returns>A new collection instance.</returns>
        public async Task<Collection?> GetCollection(string id) {
            return await GetCollection(id, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the photos collection corresponding to the given id.
        /// </summary>
        /// <param name="id">Collection's id to find.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A new collection instance.</returns>
        public async Task<Collection?> GetCollection(string id, CancellationToken cancellationToken) {
            var url = string.Format("{0}/{1}", GetUrl("collections"), id);
            var response = await Fetch(url, cancellationToken);

            if (response == null) return null;

            using var document = JsonDocument.Parse(response);
            return ExtractCollection(document.RootElement);
        }

        /// <summary>
        /// Retrieve a collection’s photos.
        /// </summary>
        /// <param name="id">The collection’s ID.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <returns>A list of photos in the collection</returns>
        public async Task<List<Photo>> GetCollectionPhotos(string id, int page = 1, int perPage = 10) {
            return await GetCollectionPhotos(id, page, perPage, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve a collection's photos.
        /// </summary>
        /// <param name="id">The collection's ID.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of photos in the collection</returns>
        public async Task<List<Photo>> GetCollectionPhotos(string id, int page, int perPage, CancellationToken cancellationToken) {
            var url = string.Format(
                "{0}/{1}/photos?page={2}&per_page={3}",
                GetUrl("collections"), id, page, perPage);

            return await FetchPhotosList(url, cancellationToken);
        }

        /// <summary>
        /// Get a single page from the list of all collections.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <returns>List of recent collections</returns>
        public async Task<List<Collection>> ListCollections(int page = 1, int perPage = 10) {
            return await ListCollections(page, perPage, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page from the list of all collections.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>List of recent collections</returns>
        public async Task<List<Collection>> ListCollections(int page, int perPage, CancellationToken cancellationToken) {
            var url = string.Format(
                "{0}?page={1}&per_page={2}",
                GetUrl("collections"), page, perPage);

            return await FetchCollectionsList(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page from the list of featured collections.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <returns>A list of collections.</returns>
        public async Task<List<Collection>> ListFeaturedCollections(int page = 1, int perPage = 10) {
            return await ListFeaturedCollections(page, perPage, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page from the list of featured collections.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of collections.</returns>
        public async Task<List<Collection>> ListFeaturedCollections(int page, int perPage, CancellationToken cancellationToken) {
            var url = string.Format(
                "{0}/featured?page={1}&per_page={2}",
                GetUrl("collections"), page, perPage);

            return await FetchCollectionsList(url, cancellationToken);
        }

        /// <summary>
        /// Get a collection's related collections.
        /// </summary>
        /// <param name="id">The collection’s ID.</param>
        /// <returns>A list of collections.</returns>
        public async Task<List<Collection>> ListRelatedCollections(string id) {
            return await ListRelatedCollections(id, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a collection's related collections.
        /// </summary>
        /// <param name="id">The collection's ID.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of collections.</returns>
        public async Task<List<Collection>> ListRelatedCollections(string id, CancellationToken cancellationToken) {
            var url = string.Format("{0}/{1}/related", GetUrl("collections"), id);
            return await FetchCollectionsList(url, cancellationToken);
        }

        #endregion collections

        #region user
        /// <summary>
        /// Return the User corresponding to the given username.
        /// </summary>
        /// <param name="username">The user’s username to find. Required.</param>
        /// <param name="width">Profile image width in pixels.</param>
        /// <param name="height">Profile image height in pixels.</param>
        /// <returns>User corresponding to the username if found.</returns>
        public async Task<User?> GetUser(string username, int width = 0, int height = 0) {
            return await GetUser(username, width, height, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Return the User corresponding to the given username.
        /// </summary>
        /// <param name="username">The user's username to find. Required.</param>
        /// <param name="width">Profile image width in pixels.</param>
        /// <param name="height">Profile image height in pixels.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>User corresponding to the username if found.</returns>
        public async Task<User?> GetUser(string username, int width, int height, CancellationToken cancellationToken) {
            var url = string.Format("{0}/{1}", GetUrl("users"), username);

            if (width != 0) { url = AddQueryString(url, "w", width); }
            if (height != 0) { url = AddQueryString(url, "w", height); }

            var response = await Fetch(url, cancellationToken);

            if (response == null) return null;

            using var document = JsonDocument.Parse(response);
            return ExtractUser(document.RootElement);
        }

        /// <summary>
        /// Retrieve a single user’s portfolio link.
        /// </summary>
        /// <param name="username">User's name to get portfolio link from.</param>
        /// <returns>The user's porfolio link</returns>
        public async Task<string?> GetUserPorfolioLink(string username) {
            var url = string.Format("{0}/{1}/portfolio", GetUrl("users"), username);
            var response = await Fetch(url);

            if (response == null) return null;

            using var document = JsonDocument.Parse(response);
            return document.RootElement.GetString("url");
        }

        /// <summary>
        /// Get a list of photos uploaded by a user.
        /// </summary>
        /// <param name="username">Username to get photos from</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="orderBy">How to sort the photos.</param>
        /// <param name="stats">Show the stats for each user’s photo.</param>
        /// <param name="quantity">The amount of for each stat.</param>
        /// <returns>A list of user's photos.</returns>
        public async Task<List<Photo>> ListUserPhotos(string username, int page = 1, int perPage = 10, 
            OrderBy orderBy = OrderBy.Latest, bool stats = false, int quantity = 30) {

            var url = string.Format(
                "{0}/{1}/photos?page={2}&per_page={3}&order_by={4}&stats={5}&quantity={6}", 
                GetUrl("users"), username, page, perPage, ConvertOderBy(orderBy), stats, quantity);

            return await FetchPhotosList(url);
        }

        /// <summary>
        /// Get a list of photos liked by a user.
        /// </summary>
        /// <param name="username">Username to get photos from</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="orderBy">How to sort the photos.</param>
        /// <returns>A list of user's liked photos.</returns>
        public async Task<List<Photo>> ListUserLikedPhotos(string username, int page = 1, 
            int perPage = 10, OrderBy orderBy = OrderBy.Latest) {

            var url = string.Format(
                "{0}/{1}/likes?page={2}&per_page={3}&order_by={4}", 
                GetUrl("users"), username, page, perPage, ConvertOderBy(orderBy));

            return await FetchPhotosList(url);
        }

        /// <summary>
        /// Get all photos collections of an user.
        /// </summary>
        /// <param name="username">Name of the user to get collections from</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <returns>List of user's collections</returns>
        public async Task<List<Collection>> ListUserCollections(string username, int page = 1, int perPage = 10) {
            var url = string.Format(
                "{0}/{1}/collections?page={2}&per_page={3}", 
                GetUrl("users"), username, page, perPage);

            return await FetchCollectionsList(url);
        }

        /// <summary>
        /// Retrieve the consolidated number of downloads, 
        /// views and likes of all user’s photos, 
        /// as well as the historical breakdown and average of these stats 
        /// in a specific timeframe (default is 30 days).
        /// </summary>
        /// <param name="username">The user’s username. Required.</param>
        /// <param name="resolution">The frequency of the stats. (Optional; default: “days”).</param>
        /// <param name="quantity">The amount of for each stat. (Optional; default: 30)</param>
        /// <returns>User's statistics</returns>
        public async Task<UserStats?> GetUserStats(
            string username,
            Resolution resolution = Resolution.Days,
            int quantity = 30) {

            var url = string.Format("{0}/{1}/statistics?resolution={2}&quantity={3}",
                GetUrl("users"), username, ConvertResolution(resolution), quantity);

            try {
                string? responseBodyAsText = await Fetch(url).ConfigureAwait(false);
                if (responseBodyAsText == null) return null;

                using var document = JsonDocument.Parse(responseBodyAsText);
                var data = document.RootElement;

                return new UserStats() {
                    Username = data.GetString("username") ?? string.Empty,
                    Downloads = ExtractStatsData(data, "downloads") ?? new StatsData(),
                    Views = ExtractStatsData(data, "views") ?? new StatsData(),
                    Likes = ExtractStatsData(data, "likes") ?? new StatsData()
                };

            } catch {
                return null;
            }
        }

        #endregion user

        #region search

        /// <summary>
        /// Get a single page of photo results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <returns>A list of photos found.</returns>
        public async Task<List<Photo>> SearchPhotos(string query, int page = 1, int perPage = 10) {
            return await SearchPhotos(query, page, perPage, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page of photo results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of photos found.</returns>
        public async Task<List<Photo>> SearchPhotos(string query, int page, int perPage, CancellationToken cancellationToken) {
            _logger.LogInformation("Searching photos with query '{Query}', page {Page}, perPage {PerPage}",
                query, page, perPage);

            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}",
                GetUrl("search_photos"), query, page, perPage);

            LastPhotosSearchQuery = query;
            var results = await FetchSearchPhotosList(url, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Photo search completed. Found {ResultCount} photos, total results: {TotalResults}",
                results.Count, LastPhotosSearchTotalResults);

            return results;
        }

        /// <summary>
        /// Get a single page of photo results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="collectionId">Collection ID to narrow search.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <returns>A list of photos found.</returns>
        public async Task<List<Photo>> SearchPhotos(string query, string collectionId, int page = 1, int perPage = 10) {
            return await SearchPhotos(query, collectionId, page, perPage, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page of photo results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="collectionId">Collection ID to narrow search.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of photos found.</returns>
        public async Task<List<Photo>> SearchPhotos(string query, string collectionId, int page, int perPage, CancellationToken cancellationToken) {
            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}&collections={4}",
                GetUrl("search_photos"), query, page, perPage, collectionId);

            LastPhotosSearchQuery = query;
            return await FetchSearchPhotosList(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page of photo results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="collectionIds">Collection ID's to narrow search.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <returns>A list of photos found.</returns>
        public async Task<List<Photo>> SearchPhotos(string query, string[] collectionIds, int page = 1, int perPage = 10) {
            return await SearchPhotos(query, collectionIds, page, perPage, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page of photo results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="collectionIds">Collection ID's to narrow search.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of photos found.</returns>
        public async Task<List<Photo>> SearchPhotos(string query, string[] collectionIds, int page, int perPage, CancellationToken cancellationToken) {
            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}&collections={4}",
                GetUrl("search_photos"), query, page, perPage, string.Join(",", collectionIds));

            LastPhotosSearchQuery = query;
            return await FetchSearchPhotosList(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page of collection results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <returns>A list of collections found.</returns>
        public async Task<List<Collection>> SearchCollections(string query, int page = 1, int perPage = 10) {
            return await SearchCollections(query, page, perPage, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page of collection results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of collections found.</returns>
        public async Task<List<Collection>> SearchCollections(string query, int page, int perPage, CancellationToken cancellationToken) {
            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}",
                GetUrl("search_collections"), query, page, perPage);

            LastCollectionsSearchQuery = query;
            return await FetchSearcCollectionsList(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single page of user results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <returns>A list of users found.</returns>
        public async Task<List<User>> SearchUsers(string query, int page = 1, int perPage = 10) {
            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}",
                GetUrl("search_users"), query, page, perPage);

            LastUsersSearchQuery = query;
            return await FetchSearcUsersList(url);
        }

        /// <summary>
        /// Return a list of found photos from a search query.
        /// </summary>
        /// <param name="url">API endpoint to fetch the photos' list. Must be a search url.</param>
        /// <returns>A list of photos found.</returns>
        public async Task<List<Photo>> FetchSearchPhotosList(string url) {
            return await FetchSearchPhotosList(url, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Return a list of found photos from a search query.
        /// </summary>
        /// <param name="url">API endpoint to fetch the photos' list. Must be a search url.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of photos found.</returns>
        public async Task<List<Photo>> FetchSearchPhotosList(string url, CancellationToken cancellationToken) {
            var listPhotos = new List<Photo>();

            try {
                var responseBodyAsText = await Fetch(url, cancellationToken).ConfigureAwait(false);
                if (responseBodyAsText == null) {
                    LastPhotosSearchTotalResults = 0;
                    LastPhotosSearchTotalPages = 0;
                    return listPhotos;
                }

                using var document = JsonDocument.Parse(responseBodyAsText);
                var data = document.RootElement;

                LastPhotosSearchTotalResults = data.GetInt32("total");
                LastPhotosSearchTotalPages = data.GetInt32("total_pages");

                if (data.TryGetProperty("results", out var resultsElement) &&
                    resultsElement.ValueKind == JsonValueKind.Array) {
                    foreach (var photoElement in resultsElement.EnumerateArray()) {
                        var photo = ExtractPhoto(photoElement);
                        if (photo != null) {
                            listPhotos.Add(photo);
                        }
                    }
                }

                return listPhotos;

            } catch /*(HttpRequestException hre)*/ {
                LastPhotosSearchTotalResults = 0;
                LastPhotosSearchTotalPages = 0;
                return listPhotos;
            }
        }

        /// <summary>
        /// Return a list of found collections from a search query.
        /// </summary>
        /// <param name="url">API endpoint to fetch the collections' list. Must be a search url.</param>
        /// <returns>A list of collections found.</returns>
        public async Task<List<Collection>> FetchSearcCollectionsList(string url) {
            return await FetchSearcCollectionsList(url, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Return a list of found collections from a search query.
        /// </summary>
        /// <param name="url">API endpoint to fetch the collections' list. Must be a search url.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of collections found.</returns>
        public async Task<List<Collection>> FetchSearcCollectionsList(string url, CancellationToken cancellationToken) {
            var listCollections = new List<Collection>();

            try {
                var responseBodyAsText = await Fetch(url, cancellationToken);
                if (responseBodyAsText == null) {
                    LastCollectionsSearchTotalResults = 0;
                    LastCollectionsSearchTotalPages = 0;
                    return listCollections;
                }

                using var document = JsonDocument.Parse(responseBodyAsText);
                var data = document.RootElement;

                LastCollectionsSearchTotalResults = data.GetInt32("total");
                LastCollectionsSearchTotalPages = data.GetInt32("total_pages");

                if (data.TryGetProperty("results", out var resultsElement) &&
                    resultsElement.ValueKind == JsonValueKind.Array) {
                    foreach (var collectionElement in resultsElement.EnumerateArray()) {
                        var collection = ExtractCollection(collectionElement);
                        if (collection != null) {
                            listCollections.Add(collection);
                        }
                    }
                }

                return listCollections;

            } catch /*(HttpRequestException hre)*/ {
                LastCollectionsSearchTotalResults = 0;
                LastCollectionsSearchTotalPages = 0;
                return listCollections;
            }
        }

        /// <summary>
        /// Return a list of found users from a search query.
        /// </summary>
        /// <param name="url">API endpoint to fetch the users' list. Must be a search url.</param>
        /// <returns>A list of users found.</returns>
        public async Task<List<User>> FetchSearcUsersList(string url) {
            var listUsers = new List<User>();

            try {
                var responseBodyAsText = await Fetch(url);
                if (responseBodyAsText == null) {
                    LastUsersSearchTotalResults = 0;
                    LastUsersSearchTotalPages = 0;
                    return listUsers;
                }

                using var document = JsonDocument.Parse(responseBodyAsText);
                var data = document.RootElement;

                LastUsersSearchTotalResults = data.GetInt32("total");
                LastUsersSearchTotalPages = data.GetInt32("total_pages");

                if (data.TryGetProperty("results", out var resultsElement) &&
                    resultsElement.ValueKind == JsonValueKind.Array) {
                    foreach (var userElement in resultsElement.EnumerateArray()) {
                        var user = ExtractUser(userElement);
                        if (user != null) {
                            listUsers.Add(user);
                        }
                    }
                }

                return listUsers;

            } catch /*(HttpRequestException hre)*/ {
                LastUsersSearchTotalResults = 0;
                LastUsersSearchTotalPages = 0;
                return listUsers;
            }
        }
        #endregion search

        #region others
        /// <summary>
        /// Get a list of counts for all of Unsplash.
        /// </summary>
        /// <returns>A list of Unplash's stats.</returns>
        public async Task<UnplashTotalStats?> GetTotalStats() {
            return await GetTotalStats(CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of counts for all of Unsplash.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>A list of Unplash's stats.</returns>
        public async Task<UnplashTotalStats?> GetTotalStats(CancellationToken cancellationToken) {
            var url = GetUrl("total_stats");
            if (url == null) return null;

            var response = await Fetch(url, cancellationToken).ConfigureAwait(false);

            if (response == null) return null;

            using var document = JsonDocument.Parse(response);
            var data = document.RootElement;

            return new UnplashTotalStats() {
                Photos = data.GetDouble("photos"),
                Downloads = data.GetDouble("downloads"),
                Views = data.GetDouble("views"),
                Likes = data.GetDouble("likes"),
                Photographers = data.GetDouble("photographers"),
                Pixels = data.GetDouble("pixels"),
                DownloadsPerSecond = data.GetDouble("downloads_per_second"),
                ViewsPerSecond = data.GetDouble("views_per_second"),
                Developers = data.GetInt32("developers"),
                Applications = data.GetInt32("applications"),
                Requests = data.GetDouble("requests")
            };
        }

        /// <summary>
        /// Get the overall Unsplash stats for the past 30 days.
        /// </summary>
        /// <returns>A list of Unplash's stats.</returns>
        public async Task<UnplashMonthlyStats?> GetMonthlyStats() {
            var url = GetUrl("monthly_stats");
            if (url == null) return null;

            var response = await Fetch(url).ConfigureAwait(false);

            if (response == null) return null;

            using var document = JsonDocument.Parse(response);
            var data = document.RootElement;

            return new UnplashMonthlyStats() {
                Downloads = data.GetDouble("downloads"),
                Views = data.GetDouble("views"),
                Likes = data.GetDouble("likes"),
                NewPhotos = data.GetDouble("new_photos"),
                NewPhotographers = data.GetDouble("new_photographers"),
                NewPixels = data.GetDouble("new_pixels"),
                NewDevelopers = data.GetInt32("new_developers"),
                NewApplications = data.GetInt32("new_applications"),
                NewRequests = data.GetDouble("new_requests")
            };
        }
        #endregion others

        // ---------------
        // PRIVATE METHODS
        // ---------------
        #region private methods
        /// <summary>
        /// Get the Unplash URL according to the specified type.
        /// </summary>
        /// <param name="type">URL shortname to retrieve.</param>
        /// <returns>An URL</returns>
        private string? GetUrl(string type) {
            switch (type) {
                case "photos":
                    return URIBase + URIEndpoints["photos"];
                case "users":
                    return URIBase + URIEndpoints["users"];
                case "search":
                    return URIBase + URIEndpoints["search"];
                case "collections":
                    return URIBase + URIEndpoints["collections"];
                case "search_photos":
                    return URIBase + URIEndpoints["search_photos"];
                case "search_collections":
                    return URIBase + URIEndpoints["search_collections"];
                case "search_users":
                    return URIBase + URIEndpoints["search_users"];
                case "total_stats":
                    return URIBase + URIEndpoints["total_stats"];
                case "monthly_stats":
                    return URIBase + URIEndpoints["monthly_stats"];
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get HttpClient instance using IHttpClientFactory if available, otherwise fall back to legacy pattern
        /// </summary>
        /// <returns>HttpClient instance</returns>
        private HttpClient GetHttpClient() {
            if (_httpClientFactory != null) {
                _logger.LogDebug("Using IHttpClientFactory to create HttpClient");
                var client = _httpClientFactory.CreateClient("unsplash");

                // Set authorization header if not already set
                if (client.DefaultRequestHeaders.Authorization == null) {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Client-ID", ApplicationId);
                }

                return client;
            }

            // Fall back to legacy HttpClient management
            var appID = ApplicationId ?? "";
            return httpClients.GetOrAdd(appID, (key) =>
            {
                return new Lazy<HttpClient>(() =>
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Client-ID", key);
                    _logger.LogDebug("Created new HttpClient for application ID {ApplicationId} (legacy pattern)", key);
                    return client;
                });
            }).Value;
        }

        /// <summary>
        /// Get a string body response async, and update rate limits.
        /// This method maintains backward compatibility by returning null on errors.
        /// </summary>
        /// <param name="url">URL to reach.</param>
        /// <returns>Body response as string, or null if an error occurred.</returns>
        private async Task<string?> Fetch(string url) {
            return await Fetch(url, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a string body response async, and update rate limits.
        /// This method maintains backward compatibility by returning null on errors.
        /// </summary>
        /// <param name="url">URL to reach.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>Body response as string, or null if an error occurred.</returns>
        private async Task<string?> Fetch(string url, CancellationToken cancellationToken) {
            try {
                return await FetchWithExceptions(url, cancellationToken).ConfigureAwait(false);
            } catch (UnsplasharpException ex) {
                _logger.LogError(ex, "HTTP request failed for URL {Url} after retries. Context: {Context}",
                    url, ex.Context?.ToSummary());
                return null;
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error during HTTP request to {Url} after retries", url);
                return null;
            }
        }

        /// <summary>
        /// Get a string body response async with comprehensive error handling.
        /// This method throws specific exceptions for different error scenarios.
        /// </summary>
        /// <param name="url">URL to reach.</param>
        /// <returns>Body response as string.</returns>
        /// <exception cref="UnsplasharpException">Thrown when the request fails</exception>
        private async Task<string> FetchWithExceptions(string url) {
            return await FetchWithExceptions(url, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a string body response async with comprehensive error handling.
        /// This method throws specific exceptions for different error scenarios.
        /// </summary>
        /// <param name="url">URL to reach.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
        /// <returns>Body response as string.</returns>
        /// <exception cref="UnsplasharpException">Thrown when the request fails</exception>
        private async Task<string> FetchWithExceptions(string url, CancellationToken cancellationToken) {
            var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _logger.LogDebug("[{CorrelationId}] Making HTTP request to {Url}", correlationId, url);

            var startTime = DateTimeOffset.UtcNow;

            try {
                var http = GetHttpClient();

                // Use retry policy for HTTP requests
                var result = await _retryPolicy.ExecuteAsync(async (retryToken) =>
                {
                    // Combine the provided cancellation token with the retry policy's token
                    using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, retryToken);
                    var response = await http.GetAsync(url, combinedTokenSource.Token).ConfigureAwait(false);

                    // Check for error status codes and throw appropriate exceptions
                    if (!response.IsSuccessStatusCode) {
                        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw UnsplasharpException.FromHttpResponse(response, responseContent, ApplicationId, correlationId);
                    }

                    var responseBodyAsText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    UpdateRateLimit(response);

                    _logger.LogDebug("[{CorrelationId}] HTTP request successful. Rate limit: {RateLimitRemaining}/{MaxRateLimit}",
                        correlationId, RateLimitRemaining, MaxRateLimit);

                    return responseBodyAsText;
                }, cancellationToken).ConfigureAwait(false);

                return result;

            } catch (HttpRequestException ex) {
                var elapsed = DateTimeOffset.UtcNow - startTime;
                _logger.LogError(ex, "[{CorrelationId}] HTTP request failed for URL {Url} after retries. Elapsed: {Elapsed}ms",
                    correlationId, url, elapsed.TotalMilliseconds);
                throw UnsplasharpException.FromHttpRequestException(ex, url, "GET", ApplicationId, correlationId);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                // If the provided cancellation token was cancelled, re-throw as OperationCanceledException
                var elapsed = DateTimeOffset.UtcNow - startTime;
                _logger.LogInformation("[{CorrelationId}] HTTP request cancelled for URL {Url}. Elapsed: {Elapsed}ms",
                    correlationId, url, elapsed.TotalMilliseconds);
                throw;
            } catch (TaskCanceledException ex) {
                var elapsed = DateTimeOffset.UtcNow - startTime;
                _logger.LogWarning(ex, "[{CorrelationId}] HTTP request timed out for URL {Url} after retries. Elapsed: {Elapsed}ms",
                    correlationId, url, elapsed.TotalMilliseconds);
                throw UnsplasharpException.FromTaskCanceledException(ex, TimeSpan.FromSeconds(30), url, "GET", ApplicationId, correlationId);
            } catch (UnsplasharpException) {
                // Re-throw Unsplasharp exceptions as-is
                throw;
            } catch (Exception ex) {
                var elapsed = DateTimeOffset.UtcNow - startTime;
                _logger.LogError(ex, "[{CorrelationId}] Unexpected error during HTTP request to {Url} after retries. Elapsed: {Elapsed}ms",
                    correlationId, url, elapsed.TotalMilliseconds);
                throw ExceptionFactory.FromGenericException(ex, url, "GET", ApplicationId, correlationId);
            }
        }

        /// <summary>
        /// Return true is the string is neither null nor empty.
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <returns>True is the string is neither null nor empty.</returns>
        private bool StringNotNull(string s) {
            return !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s);
        }







        private Exif? ExtractExif(JsonElement data) {
            if (!data.TryGetProperty("exif", out var exifElement) ||
                exifElement.ValueKind == JsonValueKind.Null)
                return null;

            return new Exif() {
                Make = exifElement.GetString("make") ?? string.Empty,
                Model = exifElement.GetString("model") ?? string.Empty,
                Iso = exifElement.GetNullableInt32("iso"),
                FocalLength = exifElement.GetString("focal_length") ?? string.Empty,
                Aperture = exifElement.GetString("aperture") ?? string.Empty
            };
        }

        private Location? ExtractLocation(JsonElement data) {
            if (!data.TryGetProperty("location", out var locationElement) ||
                locationElement.ValueKind == JsonValueKind.Null)
                return null;

            return new Location() {
                Title = locationElement.GetString("title") ?? string.Empty,
                Name = locationElement.GetString("name") ?? string.Empty,
                City = locationElement.GetString("city") ?? string.Empty,
                Country = locationElement.GetString("country") ?? string.Empty,
                Position = ExtractPosition(locationElement) ?? new Position()
            };
        }

        private Position? ExtractPosition(JsonElement data) {
            if (!data.TryGetProperty("position", out var positionElement) ||
                positionElement.ValueKind == JsonValueKind.Null)
                return null;

            return new Position() {
                Latitude = positionElement.GetInt32("latitude"),
                Longitude = positionElement.GetInt32("longitude")
            };
        }

        private List<Category> ExtractCategories(JsonElement data) {
            var categories = new List<Category>();

            if (!data.TryGetProperty("categories", out var categoriesElement) ||
                categoriesElement.ValueKind != JsonValueKind.Array)
                return categories;

            foreach (var categoryElement in categoriesElement.EnumerateArray()) {
                var category = new Category() {
                    Id = categoryElement.GetString("id") ?? string.Empty,
                    Title = categoryElement.GetString("title") ?? string.Empty,
                    PhotoCount = categoryElement.GetInt32("photo_count"),
                    Links = ExtractCategoryLinks(categoryElement) ?? new CategoryLinks()
                };
                categories.Add(category);
            }

            return categories;
        }

        private CategoryLinks? ExtractCategoryLinks(JsonElement data) {
            if (!data.TryGetProperty("links", out var linksElement) ||
                linksElement.ValueKind == JsonValueKind.Null)
                return null;

            return new CategoryLinks() {
                Self = linksElement.GetString("self") ?? string.Empty,
                Photos = linksElement.GetString("photos") ?? string.Empty
            };
        }

        private User? ExtractUser(JsonElement data) {
            if (!data.TryGetProperty("user", out var userElement) ||
                userElement.ValueKind == JsonValueKind.Null)
                return null;

            return new User() {
                Id = userElement.GetString("id") ?? string.Empty,
                UpdatedAt = userElement.GetString("updated_at") ?? string.Empty,
                FirstName = userElement.GetString("first_name") ?? string.Empty,
                LastName = userElement.GetString("last_name") ?? string.Empty,
                Username = userElement.GetString("username") ?? string.Empty,
                Name = userElement.GetString("name") ?? string.Empty,
                TwitterUsername = userElement.GetString("twitter_username") ?? string.Empty,
                PortfolioUrl = userElement.GetString("portfolio_url") ?? string.Empty,
                Bio = userElement.GetString("bio") ?? string.Empty,
                Location = userElement.GetString("location") ?? string.Empty,
                TotalLikes = userElement.GetInt32("total_likes"),
                TotalPhotos = userElement.GetInt32("total_photos"),
                TotalCollections = userElement.GetInt32("total_collections"),
                FollowedByUser = userElement.GetBoolean("followed_by_user"),
                FollowersCount = userElement.GetInt32("followers_count"),
                FollowingCount = userElement.GetInt32("following_count"),
                ProfileImage = ExtractProfileImage(userElement) ?? new ProfileImage(),
                Badge = ExtractBadge(userElement) ?? new Badge(),
                Links = ExtractUserLinks(userElement) ?? new UserLinks()
            };
        }

        private ProfileImage? ExtractProfileImage(JsonElement data) {
            if (!data.TryGetProperty("profile_image", out var profileElement) ||
                profileElement.ValueKind == JsonValueKind.Null)
                return null;

            return new ProfileImage() {
                Small = profileElement.GetString("small") ?? string.Empty,
                Medium = profileElement.GetString("medium") ?? string.Empty,
                Large = profileElement.GetString("large") ?? string.Empty
            };
        }

        private Badge? ExtractBadge(JsonElement data) {
            if (!data.TryGetProperty("badge", out var badgeElement) ||
                badgeElement.ValueKind == JsonValueKind.Null)
                return null;

            return new Badge() {
                Title = badgeElement.GetString("title") ?? string.Empty,
                Primary = badgeElement.GetBoolean("primary"),
                Slug = badgeElement.GetString("slug") ?? string.Empty,
                Link = badgeElement.GetString("link") ?? string.Empty
            };
        }

        private UserLinks? ExtractUserLinks(JsonElement data) {
            if (!data.TryGetProperty("links", out var linksElement) ||
                linksElement.ValueKind == JsonValueKind.Null)
                return null;

            return new UserLinks() {
                Self = linksElement.GetString("self") ?? string.Empty,
                Html = linksElement.GetString("html") ?? string.Empty,
                Photos = linksElement.GetString("photos") ?? string.Empty,
                Likes = linksElement.GetString("likes") ?? string.Empty,
                Portfolio = linksElement.GetString("portfolio") ?? string.Empty
            };
        }

        private Urls? ExtractUrls(JsonElement data) {
            if (!data.TryGetProperty("urls", out var urlsElement) ||
                urlsElement.ValueKind == JsonValueKind.Null)
                return null;

            return new Urls() {
                Raw = urlsElement.GetString("raw") ?? string.Empty,
                Full = urlsElement.GetString("full") ?? string.Empty,
                Regular = urlsElement.GetString("regular") ?? string.Empty,
                Small = urlsElement.GetString("small") ?? string.Empty,
                Thumbnail = urlsElement.GetString("thumb") ?? string.Empty,
                Custom = urlsElement.GetString("custom") ?? string.Empty
            };
        }

        private PhotoLinks? ExtractPhotoLinks(JsonElement data) {
            if (!data.TryGetProperty("links", out var linksElement) ||
                linksElement.ValueKind == JsonValueKind.Null)
                return null;

            return new PhotoLinks() {
                Download = linksElement.GetString("download") ?? string.Empty,
                DownloadLocation = linksElement.GetString("download_location") ?? string.Empty,
                Html = linksElement.GetString("html") ?? string.Empty,
                Self = linksElement.GetString("self") ?? string.Empty
            };
        }

        private StatsData? ExtractStatsData(JsonElement data, string propertyName) {
            if (!data.TryGetProperty(propertyName, out var statsElement) ||
                statsElement.ValueKind == JsonValueKind.Null)
                return null;

            return new StatsData() {
                Total = statsElement.GetInt32("total"),
                Historical = ExtractHistorical(statsElement) ?? new StatsHistorical()
            };
        }

        private Collection? ExtractCollection(JsonElement data) {
            if (data.ValueKind == JsonValueKind.Null || data.ValueKind == JsonValueKind.Undefined)
                return null;

            return new Collection() {
                Id = data.GetString("id") ?? string.Empty,
                Title = data.GetString("title") ?? string.Empty,
                Description = data.GetString("description") ?? string.Empty,
                PublishedAt = data.GetString("published_at") ?? string.Empty,
                UpdatedAt = data.GetString("updated_at") ?? string.Empty,
                IsCurated = data.GetBoolean("curated"),
                IsFeatured = data.GetBoolean("featured"),
                TotalPhotos = data.GetInt32("total_photos"),
                IsPrivate = data.GetBoolean("private"),
                ShareKey = data.GetString("share_key") ?? string.Empty,
                CoverPhoto = ExtractCoverPhoto(data) ?? new Photo(),
                User = ExtractUser(data) ?? new User(),
                Links = ExtractCollectionLinks(data) ?? new CollectionLinks()
            };
        }

        private Photo? ExtractCoverPhoto(JsonElement data) {
            if (!data.TryGetProperty("cover_photo", out var coverPhotoElement) ||
                coverPhotoElement.ValueKind == JsonValueKind.Null)
                return null;

            return ExtractPhoto(coverPhotoElement);
        }

        private CollectionLinks? ExtractCollectionLinks(JsonElement data) {
            if (!data.TryGetProperty("links", out var linksElement) ||
                linksElement.ValueKind == JsonValueKind.Null)
                return null;

            return new CollectionLinks() {
                Self = linksElement.GetString("self") ?? string.Empty,
                Html = linksElement.GetString("html") ?? string.Empty,
                Photos = linksElement.GetString("photos") ?? string.Empty,
                Related = linksElement.GetString("related") ?? string.Empty
            };
        }

        private StatsHistorical? ExtractHistorical(JsonElement data) {
            if (!data.TryGetProperty("historical", out var historicalElement) ||
                historicalElement.ValueKind == JsonValueKind.Null)
                return null;

            return new StatsHistorical() {
                Change = historicalElement.GetDouble("change"),
                Average = historicalElement.GetInt32("average"),
                Resolution = historicalElement.GetString("resolution") ?? string.Empty,
                Quantity = historicalElement.GetInt32("quantity"),
                Values = ExtractStatsValues(historicalElement)
            };
        }

        private List<StatsValue> ExtractStatsValues(JsonElement data) {
            var listStatsValues = new List<StatsValue>();

            if (!data.TryGetProperty("values", out var valuesElement) ||
                valuesElement.ValueKind != JsonValueKind.Array)
                return listStatsValues;

            foreach (var valueElement in valuesElement.EnumerateArray()) {
                var statsValue = new StatsValue() {
                    Date = valueElement.GetString("date") ?? string.Empty,
                    Value = valueElement.GetDouble("value")
                };
                listStatsValues.Add(statsValue);
            }

            return listStatsValues;
        }

        private Photo? ExtractPhoto(JsonElement data) {
            if (data.ValueKind == JsonValueKind.Null || data.ValueKind == JsonValueKind.Undefined)
                return null;

            return new Photo() {
                Id = data.GetString("id") ?? string.Empty,
                Color = data.GetString("color") ?? string.Empty,
                BlurHash = data.GetString("blur_hash") ?? string.Empty,
                CreatedAt = data.GetString("created_at") ?? string.Empty,
                UpdatedAt = data.GetString("updated_at") ?? string.Empty,
                Description = data.GetString("description") ?? string.Empty,
                Downloads = data.GetInt32("downloads"),
                Likes = data.GetInt32("likes"),
                IsLikedByUser = data.GetBoolean("liked_by_user"),
                Width = data.GetInt32("width"),
                Height = data.GetInt32("height"),

                Exif = ExtractExif(data) ?? new Exif(),

                Location = ExtractLocation(data) ?? new Location(),

                Urls = ExtractUrls(data) ?? new Urls(),

                Categories = ExtractCategories(data),

                Links = ExtractPhotoLinks(data) ?? new PhotoLinks(),

                User = ExtractUser(data) ?? new User()
            };
        }



        private string ConvertOderBy(OrderBy orderBy) {
            switch (orderBy) {
                case OrderBy.Latest:
                    return "latest";
                case OrderBy.Oldest:
                    return "oldest";
                case OrderBy.Popular:
                    return "popular";
                default:
                    return "latest";
            }
        }

        private string ConvertOrientation(Orientation orientation) {
            switch (orientation) {
                case Orientation.Landscape:
                    return "landscape";
                case Orientation.Portrait:
                    return "portrait";
                case Orientation.Squarish:
                    return "squarish";
                default:
                    return "landscape";
            }
        }

        private string ConvertResolution(Resolution resolution) {
            switch (resolution) {
                case Resolution.Days:
                    return "days";
                default:
                    return "days";
            }
        }

        /// <summary>
        /// Add query string to the specified URL.
        /// </summary>
        /// <param name="url">URL to add query string to.</param>
        /// <param name="queryName">Query string's name.</param>
        /// <param name="queryValue">Query string's value.</param>
        /// <returns>A new URL containing the query string.</returns>
        private string AddQueryString(string url, string queryName, string queryValue) {
            var indexQuestionMark = url.LastIndexOf("?");

            if (indexQuestionMark == -1) {
                return string.Format("{0}?{1}={2}", url, queryName, queryValue);
            }

            return string.Format("{0}&{1}={2}", url, queryName, queryValue);
        }

        /// <summary>
        /// Add query string to the specified URL.
        /// </summary>
        /// <param name="url">URL to add query string to.</param>
        /// <param name="queryName">Query string's name.</param>
        /// <param name="queryValue">Query string's value.</param>
        /// <returns>A new URL containing the query string.</returns>
        private string AddQueryString(string url, string queryName, int queryValue) {
            return AddQueryString(url, queryName, queryValue.ToString());
        }

        private void UpdateRateLimit(HttpResponseMessage response) {
            if (response.Headers.TryGetValues("X-Ratelimit-Limit", out IEnumerable<string> maxRateLimit)) {
                MaxRateLimit = int.Parse(maxRateLimit.First());
            }
            if (response.Headers.TryGetValues("X-Ratelimit-Remaining", out IEnumerable<string> rateLimitRemaining)) {
                RateLimitRemaining = int.Parse(rateLimitRemaining.First());
            }
        }
        #endregion private methods
    }
}
