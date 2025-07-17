namespace Unsplasharp.Models {
    /// <summary>
    /// Unsplash statistics counts.
    /// </summary>
    public class UnplashTotalStats {
        /// <summary>
        /// All available photos.
        /// </summary>
        public double Photos { get; set; }

        /// <summary>
        /// All photos downloads.
        /// </summary>
        public double Downloads { get; set; }

        /// <summary>
        /// All photos views.
        /// </summary>
        public double Views { get; set; }

        /// <summary>
        /// All photos likes.
        /// </summary>
        public double Likes { get; set; }

        /// <summary>
        /// All photographers.
        /// </summary>
        public double Photographers { get; set; }

        /// <summary>
        /// All pixels.
        /// </summary>
        public double Pixels { get; set; }

        /// <summary>
        /// Average number of downloads per second for the past 7 days.
        /// </summary>
        public double DownloadsPerSecond { get; set; }

        /// <summary>
        /// Average number of views per second for the past 7 days.
        /// </summary>
        public double ViewsPerSecond { get; set; }

        /// <summary>
        /// All developeres.
        /// </summary>
        public int Developers { get; set; }

        /// <summary>
        /// All applications.
        /// </summary>
        public int Applications { get; set; }

        /// <summary>
        /// All requests.
        /// </summary>
        public double Requests { get; set; }
    }

    /// <summary>
    /// Unsplash statistics counts for the past 30 days.
    /// </summary>
    public class UnplashMonthlyStats {
        /// <summary>
        /// Past 30 days downloads count.
        /// </summary>
        public double Downloads { get; set; }

        /// <summary>
        /// Past 30 days views count.
        /// </summary>
        public double Views { get; set; }

        /// <summary>
        /// Past 30 days likes count.
        /// </summary>
        public double Likes { get; set; }

        /// <summary>
        /// Past 30 days new photos count.
        /// </summary>
        public double NewPhotos { get; set; }

        /// <summary>
        /// Past 30 days new photographers count.
        /// </summary>
        public double NewPhotographers { get; set; }

        /// <summary>
        /// Past 30 days new pixels count.
        /// </summary>
        public double NewPixels { get; set; }

        /// <summary>
        /// Past 30 days new developers count.
        /// </summary>
        public int NewDevelopers { get; set; }

        /// <summary>
        /// Past 30 days new applications count.
        /// </summary>
        public int NewApplications { get; set; }

        /// <summary>
        /// Past 30 days new requests count.
        /// </summary>
        public double NewRequests { get; set; }
    }
}
