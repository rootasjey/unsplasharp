using System;
using System.ComponentModel;

namespace Unsplasharp.Models {
    /// <summary>
    /// Represents an user class from Unplash API.
    /// </summary>
    public class User : INotifyPropertyChanged {
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

        /// <summary>
        /// User's twitter username
        /// </summary>
        public string TwitterUsername { get; set; }

        /// <summary>
        /// Portfolio/personal URL.
        /// </summary>
        public string PortfolioUrl { get; set; }

        /// <summary>
        /// About/bio.
        /// </summary>
        public string Bio { get; set; }

        private string _Location;
        /// <summary>
        /// User's location
        /// </summary>
        public string Location {
            get {
                return _Location;
            }
            set {
                if (_Location != value) {
                    _Location = value;
                    NotifyPropertyChanged(nameof(Location));
                }
            }
        }

        private int _TotalLikes;
        /// <summary>
        /// User's liked photos count
        /// </summary>
        public int TotalLikes {
            get {
                return _TotalLikes;
            }
            set {
                if (_TotalLikes != value) {
                    _TotalLikes = value;
                    NotifyPropertyChanged(nameof(TotalLikes));
                }
            }
        }

        private int _TotalPhotos;
        /// <summary>
        /// User's photo count
        /// </summary>
        public int TotalPhotos {
            get {
                return _TotalPhotos;
            }
            set {
                if (_TotalPhotos != value) {
                    _TotalPhotos = value;
                    NotifyPropertyChanged(nameof(TotalPhotos));
                }
            }
        }

        private int _TotalCollections;
        /// <summary>
        /// User's collections count
        /// </summary>
        public int TotalCollections {
            get {
                return _TotalCollections;
            }
            set {
                if (_TotalCollections != value) {
                    _TotalCollections = value;
                    NotifyPropertyChanged(nameof(TotalCollections));
                }
            }
        }

        private string _UpdatedAt;
        /// <summary>
        /// Last user profile update
        /// </summary>
        public string UpdatedAt {
            get {
                return _UpdatedAt;
            }
            set {
                if (_UpdatedAt != value) {
                    _UpdatedAt = value;
                    NotifyPropertyChanged(nameof(UpdatedAt));
                }
            }
        }

        private bool _FollowedByUser;
        /// <summary>
        /// True if the current authentified user follows this user
        /// </summary>
        public bool FollowedByUser {
            get {
                return _FollowedByUser;
            }
            set {
                if (_FollowedByUser != value) {
                    _FollowedByUser = value;
                    NotifyPropertyChanged(nameof(FollowedByUser));
                }
            }
        }

        private int _FollowersCount;
        /// <summary>
        /// User dollwers count
        /// </summary>
        public int FollowersCount {
            get {
                return _FollowersCount;
            }
            set {
                if (_FollowersCount != value) {
                    _FollowersCount = value;
                    NotifyPropertyChanged(nameof(FollowersCount));
                }
            }
        }

        private int _FollowingCount;
        /// <summary>
        /// Users following count
        /// </summary>
        public int FollowingCount {
            get {
                return _FollowingCount;
            }
            set {
                if (_FollowingCount != value) {
                    _FollowingCount = value;
                    NotifyPropertyChanged(nameof(FollowingCount));
                }
            }
        }

        private int _Downloads;
        /// <summary>
        /// Downloads count
        /// </summary>
        public int Downloads {
            get {
                return _Downloads;
            }
            set {
                if (_Downloads != value) {
                    _Downloads = value;
                    NotifyPropertyChanged(nameof(Downloads));
                }
            }
        }

        #endregion simple properties

        #region composed properties

        /// <summary>
        /// User's avatar
        /// </summary>
        public ProfileImage ProfileImage { get; set; }

        private Badge _Badge;
        /// <summary>
        /// User's badge
        /// </summary>
        public Badge Badge {
            get {
                return _Badge;
            }
            set {
                if (_Badge != value) {
                    _Badge = value;
                    NotifyPropertyChanged(nameof(Badge));
                }
            }
        }

        /// <summary>
        /// User's link relations
        /// </summary>
        public UserLinks Links { get; set; }
        
        #endregion composed properties

        #region events
        /// <summary>
        /// Event raised when a property is modified
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion events
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
