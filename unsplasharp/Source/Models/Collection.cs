namespace Unsplasharp.Models {
    /// <summary>
    /// Represents a collection of photos.
    /// </summary>
    public class Collection {

        #region simple properties

        /// <summary>
        /// Collection identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Collection's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Collection description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Collection publication's date.
        /// </summary>
        public string PublishedAt { get; set; }

        /// <summary>
        /// Collection's last update's date.
        /// </summary>
        public string UpdatedAt { get; set; }

        /// <summary>
        /// True if the collection is curated.
        /// </summary>
        public bool IsCurated { get; set; }

        /// <summary>
        /// True if the collection is featured.
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Photos count in the collection.
        /// </summary>
        public int TotalPhotos { get; set; }

        /// <summary>
        /// True if the collection is private (not publicly visible).
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Collection's share key.
        /// </summary>
        public string ShareKey { get; set; }

        #endregion simple properties

        #region composed properties

        /// <summary>
        /// Collection's cover photo.
        /// </summary>
        public Photo CoverPhoto { get; set; }

        /// <summary>
        /// Collection's user.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Collection links.
        /// </summary>
        public CollectionLinks Links { get; set; }

        #endregion composed properties
    }

    /// <summary>
    /// Collection's link.
    /// </summary>
    public class CollectionLinks {
        /// <summary>
        /// Link of this collection.
        /// </summary>
        public string Self { get; set; }

        /// <summary>
        /// HTML location of this collection.
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// Photo's link in this collection.
        /// </summary>
        public string Photos { get; set; }

        /// <summary>
        /// Link of related collection's.
        /// </summary>
        public string Related { get; set; }
    }
}
