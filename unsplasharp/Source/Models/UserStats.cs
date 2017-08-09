namespace Unsplasharp.Models {
    /// <summary>
    /// User's statistics.
    /// </summary>
    public class UserStats {
        /// <summary>
        /// User's username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User's downloads statistics.
        /// </summary>
        public StatsData Downloads { get; set; }

        /// <summary>
        /// User's views statistics.
        /// </summary>
        public StatsData Views { get; set; }

        /// <summary>
        /// User's likes statistics.
        /// </summary>
        public StatsData Likes { get; set; }
    }
}
