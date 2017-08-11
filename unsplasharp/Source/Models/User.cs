namespace Unsplasharp.Models {
    /// <summary>
    /// Represents an user class from Unplash API.
    /// </summary>
    public class User {
        #region simple properties
        /// <summary>
        /// Unique user's identifer
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User's name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name.
        /// </summary>
        public string LastName { get; set; }

        public string TwitterUsername { get; set; }

        /// <summary>
        /// Portfolio/personal URL.
        /// </summary>
        public string PortfolioUrl { get; set; }

        /// <summary>
        /// About/bio.
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// User's location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// User's liked photos count
        /// </summary>
        public int TotalLikes { get; set; }

        /// <summary>
        /// User's photo count
        /// </summary>
        public int TotalPhotos { get; set; }

        /// <summary>
        /// User's collections count
        /// </summary>
        public int TotalCollections { get; set; }

        /// <summary>
        /// Last user profile update
        /// </summary>
        public string UpdatedAt { get; set; }

        /// <summary>
        /// True if the current authentified user follows this user
        /// </summary>
        public bool FollowedByUser { get; set; }

        /// <summary>
        /// User dollwers count
        /// </summary>
        public int FollowersCount { get; set; }

        /// <summary>
        /// Users following count
        /// </summary>
        public int FollowingCount { get; set; }

        /// <summary>
        /// Downloads count
        /// </summary>
        public int Downloads { get; set; }

        #endregion simple properties

        #region composed properties

        /// <summary>
        /// User's avatar
        /// </summary>
        public ProfileImage ProfileImage { get; set; }

        /// <summary>
        /// User's badge
        /// </summary>
        public Badge Badge { get; set; }

        /// <summary>
        /// User's link relations
        /// </summary>
        public UserLinks Links { get; set; }
        #endregion composed properties
    }

    /// <summary>
    /// Profile image at different sizes.
    /// </summary>
    public class ProfileImage {
        /// <summary>
        /// Small size profile image.
        /// </summary>
        public string Small { get; set; }

        /// <summary>
        /// Medium size profile image.
        /// </summary>
        public string Medium { get; set; }

        /// <summary>
        /// Large size profile image.
        /// </summary>
        public string Large { get; set; }
    }

    /// <summary>
    /// Badge (a user specific role).
    /// </summary>
    public class Badge {
        /// <summary>
        /// Badge's title (e.g. book contributor).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// True if it's the primary badge.
        /// </summary>
        public bool Primary { get; set; }

        /// <summary>
        /// Badge's description.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Badge's page's link.
        /// </summary>
        public string Link { get; set; }
    }

    /// <summary>
    /// User's link relations.
    /// </summary>
    public class UserLinks {
        /// <summary>
        /// API location of this user.
        /// </summary>
        public string Self { get; set; }

        /// <summary>
        /// HTML location of this user.
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// API location of this user's photo.
        /// </summary>
        public string Photos { get; set; }

        /// <summary>
        /// API location of this user's liked photo.
        /// </summary>
        public string Likes { get; set; }

        /// <summary>
        /// API location of this user's portfolio.
        /// </summary>
        public string Portfolio { get; set; }
    }
}
