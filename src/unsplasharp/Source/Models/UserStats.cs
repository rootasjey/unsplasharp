namespace Unsplasharp.Models {
    /// <summary>
    /// User's statistics.
    /// </summary>
    public class UserStats {
        /// <summary>
        /// User's username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's downloads statistics.
        /// </summary>
        public StatsData Downloads { get; set; } = new();

        /// <summary>
        /// User's views statistics.
        /// </summary>
        public StatsData Views { get; set; } = new();

        /// <summary>
        /// User's likes statistics.
        /// </summary>
        public StatsData Likes { get; set; } = new();
    }
}
