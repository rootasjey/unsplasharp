using System;
using System.ComponentModel;

namespace Unsplasharp.Models {
    /// <summary>
    /// Represents a collection of photos.
    /// </summary>
    public class Collection : INotifyPropertyChanged {

        #region simple properties

        /// <summary>
        /// Collection identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Collection's title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Collection description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Collection publication's date.
        /// </summary>
        public string PublishedAt { get; set; } = string.Empty;

        private string _UpdatedAt = string.Empty;
        /// <summary>
        /// Collection's last update's date.
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

        private bool _IsCurated;
        /// <summary>
        /// True if the collection is curated.
        /// </summary>
        public bool IsCurated {
            get {
                return _IsCurated;
            }
            set {
                if (_IsCurated != value) {
                    _IsCurated = value;
                    NotifyPropertyChanged(nameof(IsCurated));
                }
            }
        }

        private bool _IsFeatured;
        /// <summary>
        /// True if the collection is featured.
        /// </summary>
        public bool IsFeatured {
            get {
                return _IsFeatured;
            }
            set {
                if (_IsFeatured != value) {
                    _IsFeatured = value;
                    NotifyPropertyChanged(nameof(IsFeatured));
                }
            }
        }

        private int _TotalPhotos;
        /// <summary>
        /// Photos count in the collection.
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


        private bool _IsPrivate;
        /// <summary>
        /// True if the collection is private (not publicly visible).
        /// </summary>
        public bool IsPrivate {
            get {
                return _IsPrivate;
            }
            set {
                if (_IsPrivate != value) {
                    _IsPrivate = value;
                    NotifyPropertyChanged(nameof(IsPrivate));
                }
            }
        }

        /// <summary>
        /// Collection's share key.
        /// </summary>
        public string ShareKey { get; set; } = string.Empty;

        #endregion simple properties

        #region composed properties

        /// <summary>
        /// Collection's cover photo.
        /// </summary>
        public Photo CoverPhoto { get; set; } = new();

        /// <summary>
        /// Collection's user.
        /// </summary>
        public User User { get; set; } = new();

        /// <summary>
        /// Collection links.
        /// </summary>
        public CollectionLinks Links { get; set; } = new();

        #endregion composed properties

        #region events
        /// <summary>
        /// Event raised when a property is modified
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged(String propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion events
    }

    /// <summary>
    /// Collection's link.
    /// </summary>
    public class CollectionLinks {
        /// <summary>
        /// Link of this collection.
        /// </summary>
        public string Self { get; set; } = string.Empty;

        /// <summary>
        /// HTML location of this collection.
        /// </summary>
        public string Html { get; set; } = string.Empty;

        /// <summary>
        /// Photo's link in this collection.
        /// </summary>
        public string Photos { get; set; } = string.Empty;

        /// <summary>
        /// Link of related collection's.
        /// </summary>
        public string Related { get; set; } = string.Empty;
    }
}
