namespace Unsplasharp.Models {
    /// <summary>
    /// Photo's statistics.
    /// </summary>
    public class PhotoStats {
        /// <summary>
        /// Photo's statistics id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Photo's downloads statistics.
        /// </summary>
        public StatsData Downloads { get; set; }

        /// <summary>
        /// Photo's views statistics.
        /// </summary>
        public StatsData Views { get; set; }

        /// <summary>
        /// Photo's likes statistics.
        /// </summary>
        public StatsData Likes { get; set; }
    }
}
