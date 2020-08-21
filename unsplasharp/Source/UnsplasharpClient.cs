using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Unsplasharp.Models;
using System.Linq;

namespace Unsplasharp {
    /// <summary>
    /// Represents a Unsplasharp class which can be used to communicate with Unplash APIs.
    /// </summary>
    public class UnsplasharpClient {
        #region variables
        /// <summary>
        /// API Key 
        /// </summary>
        public string Secret { get; set; }

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
        /// Unplash endpoints
        /// </summary>
        private static IDictionary<string, string> URIEndpoints = new Dictionary<string, string>() {
            {"photos", "photos" },
            {"curated_photos", "photos/curated" },
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
        public string LastPhotosSearchQuery { get; set; }

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
        public string LastCollectionsSearchQuery { get; set; }

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
        public string LastUsersSearchQuery { get; set; }

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
        public UnsplasharpClient(string applicationId, string secret = "") {
            ApplicationId = applicationId;
            Secret = secret;
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
        public async Task<Photo> GetPhoto(string id, int width = 0, int height = 0) {
            var url = string.Format("{0}/{1}", GetUrl("photos"), id);

            if (width != 0) { url = AddQueryString(url, "w", width); }
            if (height != 0) { url = AddQueryString(url, "h", height); }

            return await FetchPhoto(url);
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
        public async Task<Photo> GetPhoto(string id, int width, int height, int rectX, int rectY, int rectWidth, int rectHeight) {
            var url = string.Format("{0}/{1}?w={2}&h={3}&rect={4},{5},{6},{7}", 
                                    GetUrl("photos"), id, width, height, rectX, rectY, rectWidth, rectHeight);

            return await FetchPhoto(url);
        }

        /// <summary>
        /// Retrieve a single random photo.
        /// </summary>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo> GetRandomPhoto() {
            var url = string.Format("{0}/random", GetUrl("photos"));
            return await FetchPhoto(url);
        }

        /// <summary>
        /// Retrieve a single random photo from a specific collection.
        /// </summary>
        /// <param name="collectionId">Public collection ID to filter selection.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo> GetRandomPhoto(string collectionId) {
            var url = string.Format("{0}/random?collections={1}", GetUrl("photos"), collectionId);
            return await FetchPhoto(url);
        }

        /// <summary>
        /// Retrieve a single random photo from specific collections.
        /// </summary>
        /// <param name="collectionIds">Public collection ID(‘s) to filter selection.</param>
        /// <returns>A new Photo class instance.</returns>
        public async Task<Photo> GetRandomPhoto(string[] collectionIds) {
            var url = string.Format("{0}/random?collections={1}", 
                GetUrl("photos"), string.Join(",", collectionIds));

            return await FetchPhoto(url);
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

            return await FetchPhotosList(url);
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

            return await FetchPhotosList(url);
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
        public async Task<PhotoStats> GetPhotoStats(string id, Resolution resolution = Resolution.Days, int quantity = 30) {
            var url = string.Format(
                "{0}/{1}/statistics?resolution={2}&quantity={3}", 
                GetUrl("photos"), id, ConvertResolution(resolution), quantity);

            try {
                string responseBodyAsText = await Fetch(url);
                var data = JObject.Parse(responseBodyAsText);

                return new PhotoStats() {
                    Id = (string)data["id"],
                    Downloads = new StatsData() {
                        Total = int.Parse((string)data["downloads"]["total"]),
                        Historical = ExtractHistorical(data["downloads"])
                    },
                    Views = new StatsData() {
                        Total = int.Parse((string)data["views"]["total"]),
                        Historical = ExtractHistorical(data["views"])
                    },
                    Likes = new StatsData() {
                        Total = int.Parse((string)data["likes"]["total"]),
                        Historical = ExtractHistorical(data["likes"])
                    }
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
        public async Task<string> GetPhotoDownloadLink(string id) {
            var url = string.Format("{0}/{1}/download", GetUrl("photos"), id);
            var response = await Fetch(url);

            if (response == null) return null;
            var data = JObject.Parse(response);

            return (string)data["url"];
        }

        /// <summary>
        /// Returns a list of all  photos.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <param name="orderBy">How to sort the photos.</param>
        /// <returns>List of all  photos.</returns>
        public async Task<List<Photo>> ListPhotos(int page = 1, int perPage = 10, OrderBy orderBy = OrderBy.Latest) {
            var url = string.Format(
                "{0}?page={1}&per_page={2}&order_by={3}", 
                GetUrl("photos"), page, perPage, ConvertOderBy(orderBy));

            return await FetchPhotosList(url);
        }

        /// <summary>
        /// Returns a list of curated photos.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// // <param name="perPage">Number of items per page.</param>
        /// <param name="orderBy">How to sort the photos.</param>
        /// <returns>List of curated photos.</returns>
        public async Task<List<Photo>> ListCuratedPhotos(int page = 1, int perPage = 10, OrderBy orderBy = OrderBy.Latest) {
            var url = string.Format(
                "{0}?page={1}&per_page={2}&order_by={3}", 
                GetUrl("curated_photos"), page, perPage, ConvertOderBy(orderBy));

            return await FetchPhotosList(url);
        }

        /// <summary>
        /// Return a list of photos from the specified URL.
        /// </summary>
        /// <param name="url">API endpoint to fetch the photos' list.</param>
        /// <returns>A list of photos.</returns>
        public async Task<List<Photo>> FetchPhotosList(string url) {
            var listPhotos = new List<Photo>();

            try {
                var responseBodyAsText = await Fetch(url);
                var data = JArray.Parse(responseBodyAsText);

                foreach (JObject rawPhoto in data) {
                    var photo = ExtractPhoto(rawPhoto);
                    listPhotos.Add(photo);
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
        public async Task<Photo> FetchPhoto(string url) {
            var response = await Fetch(url);

            if (response == null) return null;
            var data = JObject.Parse(response);

            var photo = ExtractPhoto(data);
            return photo;
        }

        #endregion photos

        #region collections
        /// <summary>
        /// Returns a list of collections (of photos) from a specified URL
        /// </summary>
        /// <param name="url">API endpoint where to fetch the photos' list.</param>
        /// <returns>Collections list</returns>
        public async Task<List<Collection>> FetchCollectionsList(string url) {
            var listCollection = new List<Collection>();

            try {
                var responseBodyAsText = await Fetch(url);
                var jsonList = JArray.Parse(responseBodyAsText);

                foreach (JObject item in jsonList) {
                    var collection = ExtractCollection(item);
                    listCollection.Add(collection);
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
        public async Task<Collection> GetCollection(string id) {
            var url = string.Format("{0}/{1}", GetUrl("collections"), id);
            var response = await Fetch(url);

            if (response == null) return null;
            var data = JObject.Parse(response);

            return ExtractCollection(data);
        }

        /// <summary>
        /// Retrieve a collection’s photos.
        /// </summary>
        /// <param name="id">The collection’s ID.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <returns>A list of photos in the collection</returns>
        public async Task<List<Photo>> GetCollectionPhotos(string id, int page = 1, int perPage = 10) {
            var url = string.Format(
                "{0}/{1}/photos?page={2}&per_page={3}",
                GetUrl("collections"), id, page, perPage);

            return await FetchPhotosList(url);
        }

        /// <summary>
        /// Get a single page from the list of all collections.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <returns>List of recent collections</returns>
        public async Task<List<Collection>> ListCollections(int page = 1, int perPage = 10) {
            var url = string.Format(
                "{0}?page={1}&per_page={2}", 
                GetUrl("collections"), page, perPage);

            return await FetchCollectionsList(url);
        }

        /// <summary>
        /// Get a single page from the list of featured collections.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <returns>A list of collections.</returns>
        public async Task<List<Collection>> ListFeaturedCollections(int page = 1, int perPage = 10) {
            var url = string.Format(
                "{0}/featured?page={1}&per_page={2}", 
                GetUrl("collections"), page, perPage);

            return await FetchCollectionsList(url);
        }

        /// <summary>
        /// Get a single page from the list of curated collections.
        /// </summary>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page. (Max: 30)</param>
        /// <returns>A list of collections.</returns>
        public async Task<List<Collection>> ListCuratedCollections(int page = 1, int perPage = 10) {
            var url = string.Format("" +
                "{0}/curated?page={1}&per_page={2}", 
                GetUrl("collections"), page, perPage);

            return await FetchCollectionsList(url);
        }

        /// <summary>
        /// Get a collection's related collections.
        /// </summary>
        /// <param name="id">The collection’s ID.</param>
        /// <returns>A list of collections.</returns>
        public async Task<List<Collection>> ListRelatedCollections(string id) {
            var url = string.Format("{0}/{1}/related", GetUrl("collections"), id);
            return await FetchCollectionsList(url);
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
        public async Task<User> GetUser(string username, int width = 0, int height = 0) {
            var url = string.Format("{0}/{1}", GetUrl("users"), username);

            if (width != 0) { url = AddQueryString(url, "w", width); }
            if (height != 0) { url = AddQueryString(url, "w", height); }

            var response = await Fetch(url);

            if (response == null) return null;
            var data = JObject.Parse(response);

            return ExtractUser(data);
        }

        /// <summary>
        /// Retrieve a single user’s portfolio link.
        /// </summary>
        /// <param name="username">User's name to get portfolio link from.</param>
        /// <returns>The user's porfolio link</returns>
        public async Task<string> GetUserPorfolioLink(string username) {
            var url = string.Format("{0}/{1}/portfolio", GetUrl("users"), username);
            var response = await Fetch(url);

            if (response == null) return null;
            var data = JObject.Parse(response);

            return (string)data["url"];
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
        public async Task<UserStats> GetUserStats(
            string username, 
            Resolution resolution = Resolution.Days, 
            int quantity = 30) {

            var url = string.Format("{0}/{1}/statistics?resolution={2}&quantity={3}", 
                GetUrl("users"), username, ConvertResolution(resolution), quantity);

            try {
                string responseBodyAsText = await Fetch(url);
                var data = JObject.Parse(responseBodyAsText);

                return new UserStats() {
                    Username = (string)data["username"],
                    Downloads = new StatsData() {
                        Total = int.Parse((string)data["downloads"]["total"]),
                        Historical = ExtractHistorical(data["downloads"])
                    },
                    Views = new StatsData() {
                        Total = int.Parse((string)data["views"]["total"]),
                        Historical = ExtractHistorical(data["views"])
                    },
                    Likes = new StatsData() {
                        Total = int.Parse((string)data["likes"]["total"]),
                        Historical = ExtractHistorical(data["likes"])
                    }
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
            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}",
                GetUrl("search_photos"), query, page, perPage);

            LastPhotosSearchQuery = query;
            return await FetchSearchPhotosList(url);
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
            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}&collections={4}",
                GetUrl("search_photos"), query, page, perPage, collectionId);

            LastPhotosSearchQuery = query;
            return await FetchSearchPhotosList(url);
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
            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}&collections={4}",
                GetUrl("search_photos"), query, page, perPage, string.Join(",", collectionIds));

            LastPhotosSearchQuery = query;
            return await FetchSearchPhotosList(url);
        }

        /// <summary>
        /// Get a single page of collection results for a query.
        /// </summary>
        /// <param name="query">Search terms.</param>
        /// <param name="page">Page number to retrieve.</param>
        /// <param name="perPage">Number of items per page.</param>
        /// <returns>A list of collections found.</returns>
        public async Task<List<Collection>> SearchCollections(string query, int page = 1, int perPage = 10) {
            var url = string.Format(
                "{0}?query={1}&page={2}&per_page={3}",
                GetUrl("search_collections"), query, page, perPage);

            LastCollectionsSearchQuery = query;
            return await FetchSearcCollectionsList(url);
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
            var listPhotos = new List<Photo>();

            try {
                var responseBodyAsText = await Fetch(url);
                var data = JObject.Parse(responseBodyAsText);
                var results = (JArray)data["results"];

                LastPhotosSearchTotalResults = (int)data["total"];
                LastPhotosSearchTotalPages = (int)data["total_pages"];

                foreach (JObject item in results) {
                    var photo = ExtractPhoto(item);
                    listPhotos.Add(photo);
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
            var listCollections = new List<Collection>();

            try {
                var responseBodyAsText = await Fetch(url);
                var data = JObject.Parse(responseBodyAsText);
                var results = (JArray)data["results"];

                LastCollectionsSearchTotalResults = (int)data["total"];
                LastCollectionsSearchTotalPages = (int)data["total_pages"];

                foreach (JObject item in results) {
                    var collection = ExtractCollection(item);
                    listCollections.Add(collection);
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
                var data = JObject.Parse(responseBodyAsText);
                var results = (JArray)data["results"];

                LastUsersSearchTotalResults = (int)data["total"];
                LastUsersSearchTotalPages = (int)data["total_pages"];

                foreach (JObject item in results) {
                    var user = ExtractUser(item);
                    listUsers.Add(user);
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
        public async Task<UnplashTotalStats> GetTotalStats() {
            var url = GetUrl("total_stats");
            var response = await Fetch(url);

            if (response == null) return null;
            var data = JObject.Parse(response);

            return new UnplashTotalStats() {
                Photos = double.Parse((string)data["photos"]),
                Downloads = double.Parse((string)data["downloads"]),
                Views = double.Parse((string)data["views"]),
                Likes = double.Parse((string)data["likes"]),
                Photographers = double.Parse((string)data["photographers"]),
                Pixels = double.Parse((string)data["pixels"]),
                DownloadsPerSecond = double.Parse((string)data["downloads_per_second"]),
                ViewsPerSecond = double.Parse((string)data["views_per_second"]),
                Developers = int.Parse((string)data["developers"]),
                Applications = int.Parse((string)data["applications"]),
                Requests = double.Parse((string)data["requests"])
            };
        }

        /// <summary>
        /// Get the overall Unsplash stats for the past 30 days.
        /// </summary>
        /// <returns>A list of Unplash's stats.</returns>
        public async Task<UnplashMonthlyStats> GetMonthlyStats() {
            var url = GetUrl("monthly_stats");
            var response = await Fetch(url);

            if (response == null) return null;
            var data = JObject.Parse(response);

            return new UnplashMonthlyStats() {
                Downloads = double.Parse((string)data["downloads"]),
                Views = double.Parse((string)data["views"]),
                Likes = double.Parse((string)data["likes"]),
                NewPhotos = double.Parse((string)data["new_photos"]),
                NewPhotographers = double.Parse((string)data["new_photographers"]),
                NewPixels = double.Parse((string)data["new_pixels"]),
                NewDevelopers = int.Parse((string)data["new_developers"]),
                NewApplications = int.Parse((string)data["new_applications"]),
                NewRequests = double.Parse((string)data["new_requests"])
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
        private string GetUrl(string type) {
            switch (type) {
                case "photos":
                    return URIBase + URIEndpoints["photos"];
                case "curated_photos":
                    return URIBase + URIEndpoints["curated_photos"];
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
        /// Get a string body response async, and update rate limits.
        /// </summary>
        /// <param name="url">URL to reach.</param>
        /// <returns>Body response as string.</returns>
        private async Task<string> Fetch(string url) {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Client-ID", ApplicationId);
            HttpResponseMessage response = null;

            try {
                response = await http.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBodyAsText = await response.Content.ReadAsStringAsync();

                UpdateRateLimit(response);

                return responseBodyAsText;

            } catch {
                return null;
            }
        }

        /// <summary>
        /// Returns true if the specified data is null.
        /// </summary>
        /// <param name="data">Data to test.</param>
        /// <returns>True if the data is null</returns>
        private bool IsNull(JToken data) {
            return data == null ||
                   data.Type == JTokenType.Null ||
                   data.Type == JTokenType.String;
        }

        /// <summary>
        /// Return true is the string is neither null nor empty.
        /// </summary>
        /// <param name="s">String to test.</param>
        /// <returns>True is the string is neither null nor empty.</returns>
        private bool StringNotNull(string s) {
            return !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s);
        }

        private int ExtractNumber(JToken data) {
            if (IsNull(data)) return 0;
            return string.IsNullOrEmpty((string)data) ? 0 : int.Parse((string)data);
        }

        private bool ExtractBoolean(JToken data) {
            if (IsNull(data)) return false;
            return string.IsNullOrEmpty((string)data) ? false : (bool)data;
        }

        private List<Category> ExtractCategories(JToken data) {
            var categories = new List<Category>();
            if (IsNull(data)) return categories;

            var dataCategories = (JArray)data;

            if (dataCategories.Count == 0) return categories;

            foreach (JObject item in dataCategories) {
                var category = new Category() {
                    Id = (string)item["id"],
                    Title = (string)item["title"],
                    PhotoCount = (int)item["photo_count"],
                    Links = new CategoryLinks() {
                        Self = (string)item["links"]["self"],
                        Photos = (string)item["links"]["photos"],
                    }
                };

                categories.Add(category);
            }

            return categories;
        }

        private Badge ExtractBadge(JToken data) {
            if (IsNull(data)) return null;

            return new Badge() {
                Title = (string)data["title"],
                Primary = (bool)data["primary"],
                Slug = (string)data["slug"],
                Link = (string)data["link"]
            };
        }

        private User ExtractUser(JToken data) {
            if (IsNull(data)) return null;

            return new User() {
                Id = (string)data["id"],
                UpdatedAt = (string)data["updated_at"],
                FirstName = (string)data["first_name"],
                LastName = (string)data["last_name"],
                Username = (string)data["username"],
                Name = (string)data["name"],
                TwitterUsername = (string)data["twitter_username"],
                PortfolioUrl = (string)data["portfolio_url"],
                Bio = (string)data["bio"],
                Location = (string)data["location"],
                TotalLikes = ExtractNumber(data["total_likes"]),
                TotalPhotos = ExtractNumber(data["total_photos"]),
                TotalCollections = ExtractNumber(data["total_collections"]),
                FollowedByUser = ExtractBoolean(data["followed_by_user"]),
                FollowersCount = ExtractNumber(data["followers_count"]),
                FollowingCount = ExtractNumber(data["following_count"]),

                ProfileImage = new ProfileImage() {
                    Small = (string)data["profile_image"]["small"],
                    Medium = (string)data["profile_image"]["medium"],
                    Large = (string)data["profile_image"]["large"]
                },

                Badge = ExtractBadge(data["badge"]),

                Links = new UserLinks() {
                    Self = (string)data["links"]["self"],
                    Html = (string)data["links"]["html"],
                    Photos = (string)data["links"]["photos"],
                    Likes = (string)data["links"]["likes"],
                    Portfolio = (string)data["links"]["portfolio"],
                }
            };
        }

        private Exif ExtractExif(JToken data) {
            if (IsNull(data)) return null;

            return new Exif() {
                Make = (string)data["make"],
                Model = (string)data["model"],
                Iso = (int?)data["iso"],
                FocalLength = (string)data["focal_length"],
                Aperture = (string)data["aperture"],
            };
        }

        private Location ExtractLocation(JToken data) {
            if (IsNull(data)) return null;

            return new Location() {
                Title = (string)data["title"],
                Name = (string)data["name"],
                City = (string)data["city"],
                Country = (string)data["country"],

                Position = new Position() {
                    Latitude = IsNull(data["position"]["latitude"]) ? 0 : (int)data["position"]["latitude"],
                    Longitude = IsNull(data["position"]["longitude"]) ? 0 : (int)data["position"]["longitude"],
                }
            };
        }

        private Photo ExtractPhoto(JToken data) {
            if (IsNull(data)) return null;

            return new Photo() {
                Id = (string)data["id"],
                Color = (string)data["color"],
                CreatedAt = (string)data["created_at"],
                UpdatedAt = (string)data["updated_at"],
                Description = (string)data["description"],
                Downloads = ExtractNumber(data["downloads"]),
                Likes = ExtractNumber(data["likes"]),
                IsLikedByUser = ExtractBoolean(data["liked_by_user"]),
                Width = ExtractNumber(data["width"]),
                Height = ExtractNumber(data["height"]),

                Exif = ExtractExif(data["exif"]),

                Location = ExtractLocation(data["location"]),

                Urls = new Urls() {
                    Raw = (string)data["urls"]["raw"],
                    Full = (string)data["urls"]["full"],
                    Regular = (string)data["urls"]["regular"],
                    Small = (string)data["urls"]["small"],
                    Thumbnail = (string)data["urls"]["thumb"],
                    Custom = (string)data["urls"]["custom"]
                },

                Categories = ExtractCategories(data["categories"]),

                Links = new PhotoLinks() {
                    Download = (string)data["links"]["download"],
                    DownloadLocation = (string)data["links"]["download_location"],
                    Html = (string)data["links"]["html"],
                    Self = (string)data["links"]["self"],
                },

                User = ExtractUser(data["user"])
            };
        }

        private Collection ExtractCollection(JToken data) {
            if (IsNull(data)) return null;

            return new Collection() {
                Id = (string)data["id"],
                Title = (string)data["title"],
                Description = (string)data["description"],
                PublishedAt = (string)data["published_at"],
                UpdatedAt = (string)data["updated_at"],
                IsCurated = ExtractBoolean(data["curated"]),
                IsFeatured = ExtractBoolean(data["featured"]),
                TotalPhotos = ExtractNumber(data["total_photos"]),
                IsPrivate = ExtractBoolean(data["private"]),
                ShareKey = (string)data["share_key"],

                CoverPhoto = ExtractPhoto(data["cover_photo"]),

                User = ExtractUser(data["user"]),

                Links = new CollectionLinks() {
                    Self = (string)data["links"]["self"],
                    Html = (string)data["links"]["html"],
                    Photos = (string)data["links"]["photos"],
                    Related = (string)data["links"]["related"],
                }
            };
        }

        private StatsHistorical ExtractHistorical(JToken data) {
            if (IsNull(data)) return null;

            return new StatsHistorical() {
                Change = double.Parse((string)data["historical"]["change"]),
                Average = string.IsNullOrEmpty((string)data["historical"]["average"]) ? 
                            0 : int.Parse((string)data["historical"]["average"]),
                Resolution = (string)data["historical"]["resolution"],
                Quantity = int.Parse((string)data["historical"]["quantity"]),
                Values = ExtractStatsValues(data["historical"]["values"])
            };
        }

        private List<StatsValue> ExtractStatsValues(JToken data) {
            var listStatsValues = new List<StatsValue>();

            if (IsNull(data)) return listStatsValues;

            var dataStatsValues = (JArray)data;

            if (dataStatsValues.Count == 0) return listStatsValues;

            foreach (JObject item in dataStatsValues) {
                var statsValues = new StatsValue() {
                    Date = (string)item["date"],
                    Value = double.Parse((string)item["value"])
                };

                listStatsValues.Add(statsValues);
            }

            return listStatsValues;
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
