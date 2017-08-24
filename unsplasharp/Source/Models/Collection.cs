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

        private string _UpdatedAt;
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
