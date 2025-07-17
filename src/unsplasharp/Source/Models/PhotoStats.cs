namespace Unsplasharp.Models {
    /// <summary>
    /// Photo's statistics.
    /// </summary>
    public class PhotoStats {
        /// <summary>
        /// Photo's statistics id.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Photo's downloads statistics.
        /// </summary>
        public StatsData Downloads { get; set; } = new();

        /// <summary>
        /// Photo's views statistics.
        /// </summary>
        public StatsData Views { get; set; } = new();

        /// <summary>
        /// Photo's likes statistics.
        /// </summary>
        public StatsData Likes { get; set; } = new();
    }
}
