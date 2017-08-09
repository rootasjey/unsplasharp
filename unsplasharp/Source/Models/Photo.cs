using System.Collections.Generic;

namespace Unsplasharp.Models {
    /// <summary>
    /// Represents a photo class from Unsplash API.
    /// </summary>
    public class Photo {

        #region simple properties
        /// <summary>
        /// Photo's unique identifier composed of Unicode characters.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Photo's dezcription
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Date indicating when the photo has been created.
        /// </summary>
        public string CreatedAt { get; set; }

        /// <summary>
        /// Date indicating the last time the photo has been updated.
        /// </summary>
        public string UpdatedAt { get; set; }

        /// <summary>
        /// Photo's width in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Photo's height in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The main color composing the photo.
        /// </summary>
        public string Color { get; set; }

        private int _Downloads;

        /// <summary>
        /// Downloads count since the photo was created.
        /// </summary>
        public int Downloads {
            get { return _Downloads; }
            set { _Downloads = value; }
        }


        private int _Likes;

        /// <summary>
        /// Likes count since the photo was created.
        /// </summary>
        public int Likes {
            get {
                return _Likes;
            }
            set {
                if (_Likes != value) {
                    _Likes = value;
                }
            }
        }

        private bool _IsLikedByUser;

        /// <summary>
        /// Whether the photo has been liked by the current user if a user is logged.
        /// </summary>
        public bool IsLikedByUser {
            get { return _IsLikedByUser; }
            set { _IsLikedByUser = value; }
        }

        #endregion simple properties

        #region composed properties
        /// <summary>
        /// The photo's collection where the photo is included, if any.
        /// </summary>
        public List<Collection> CurrentUserCollection { get; set; }

        /// <summary>
        /// Absolute photo's URLs (for different photo's sizes).
        /// </summary>
        public Urls Urls { get; set; }

        /// <summary>
        /// Photo's matched categories.
        /// </summary>
        public List<Category> Categories { get; set; }

        /// <summary>
        /// Photo's owner (who's uploaded the content).
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Camera specifications.
        /// </summary>
        public Exif Exif { get; set; }

        /// <summary>
        /// Represents where the photo has been shot.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Photo's link relations
        /// </summary>
        public PhotoLinks Links { get; set; }

        #endregion composed properties
    }

    /// <summary>
    /// Photo's link relations.
    /// </summary>
    public class PhotoLinks {
        /// <summary>
        /// API location of this photo.
        /// </summary>
        public string Self { get; set; }

        /// <summary>
        /// HTML location of this photo.
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// Download location of this photo.
        /// </summary>
        public string Download { get; set; }

        /// <summary>
        ///  API Download location of this photo (check if direct download).
        /// </summary>
        public string DownloadLocation { get; set; }
    }

    /// <summary>
    /// Camera specifications.
    /// </summary>
    public class Exif {
        /// <summary>
        /// Camera’s brand.
        /// </summary>
        public string Make { get; set; }

        /// <summary>
        /// Camera’s model.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Camera’s exposure time.
        /// </summary>
        public string ExposureTime { get; set; }

        /// <summary>
        /// Camera’s aperture value.
        /// </summary>
        public string Aperture { get; set; }

        /// <summary>
        /// Camera’s focal length.
        /// </summary>
        public string FocalLength { get; set; }

        /// <summary>
        /// Camera’s iso.
        /// </summary>
        public int? Iso { get; set; }
    }

    /// <summary>
    /// The photo's location.
    /// </summary>
    public class Location {
        private string _City;

        /// <summary>
        /// The photo location’s city.
        /// </summary>
        public string City {
            get { return _City; }
            set { _City = value; }
        }

        private string _Country;

        /// <summary>
        /// The photo location’s country.
        /// </summary>
        public string Country {
            get { return _Country; }
            set { _Country = value; }
        }

        private Position _Position;

        /// <summary>
        /// The photo location’s position (latitude, longitude).
        /// </summary>
        public Position Position {
            get { return _Position; }
            set { _Position = value; }
        }
    }

    /// <summary>
    /// Represents a geographical position (latitude, longitude).
    /// </summary>
    public class Position {
        private int _Latitude;

        /// <summary>
        /// Geographical latitude.
        /// </summary>
        public int Latitude {
            get { return _Latitude; }
            set { _Latitude = value; }
        }

        private int _Longitude;

        /// <summary>
        /// Geographical longitude.
        /// </summary>
        public int Longitude {
            get { return _Longitude; }
            set { _Longitude = value; }
        }

    }

    /// <summary>
    /// Photo's URLs linking to the direct photo (full screen).
    /// </summary>
    public class Urls {
        /// <summary>
        /// URL linking to the photo in native resolution and uncompressed.
        /// </summary>
        public string Raw { get; set; }

        /// <summary>
        /// URL linking to the photo in a large size.
        /// </summary>
        public string Full { get; set; }

        /// <summary>
        /// URL linking to the photo in a medium size.
        /// </summary>
        public string Regular { get; set; }

        /// <summary>
        /// URL linking to the photo in a small size.
        /// </summary>
        public string Small { get; set; }

        /// <summary>
        /// URL linking to the photo in a thumbnail size.
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// URL linking to the photo in a custom size if specified by the user.
        /// </summary>
        public string Custom { get; set; }
    }

    /// <summary>
    /// Photo's category.
    /// </summary>
    public class Category {
        /// <summary>
        /// Category's identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Category's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Number of photos in this category.
        /// </summary>
        public int PhotoCount { get; set; }

        /// <summary>
        /// Category's links.
        /// </summary>
        public CategoryLinks Links { get; set; }
    }

    /// <summary>
    /// Photo's link to its categories.
    /// </summary>
    public class CategoryLinks {
        /// <summary>
        /// Link of this category.
        /// </summary>
        public string Self { get; set; }

        /// <summary>
        /// Link to all photos in this category.
        /// </summary>
        public string Photos { get; set; }

    }
}

