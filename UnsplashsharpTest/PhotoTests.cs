using System;
using Unsplasharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using UnsplashsharpTest.Data;
using static Unsplasharp.UnsplasharpClient;

namespace UnsplashsharpTest {
    [TestClass]
    public class PhotoTests {
        [TestMethod]
        public async Task GetPhotoTest() {
            //var id = "TPv9dh822VA";
            var id = "qcs09SwNPHY";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var photo = await client.GetPhoto(id);

            // Custom size
            var photoWidth = await client.GetPhoto(id, width: 400);
            var photoWidthHeight = await client.GetPhoto(id, width: 500, height: 500);

            // Custom size + cropped
            var photoCropped = await client.GetPhoto(id, 600, 600, 10, 10, 100, 100);

            Assert.IsNotNull(photo);
            Assert.IsNotNull(photoWidth);
            Assert.IsNotNull(photoWidthHeight);
            Assert.IsNotNull(photoCropped);

            Assert.IsNotNull(photoWidth.Urls.Custom);
            Assert.IsNotNull(photoWidthHeight.Urls.Custom);
            Assert.IsNotNull(photoCropped.Urls.Custom);
        }

        [TestMethod]
        public async Task GetRandomPhotoTestLowSafety()
        {
            await GetRandomPhotoTest(UnsplasharpClient.ContentSafety.Low);
        }

        [TestMethod]
        public async Task GetRandomPhotoTestHighSafety()
        {
            await GetRandomPhotoTest(UnsplasharpClient.ContentSafety.High);
        }

        private async Task GetRandomPhotoTest(UnsplasharpClient.ContentSafety contentFilter) {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            client.ContentFilter = contentFilter;
            var randomPhoto = await client.GetRandomPhoto();

            var randomPhotoFromCollection = await client.GetRandomPhoto("499830");
            var randomPhotoFromCollections = await client.GetRandomPhoto(new string[] { "499830", "194162" });

            var randomPhotoFromUser = await client.GetRandomPhoto(1, username: "chrisjoelcampbell");
            var randomPhotosFromQuery = await client.GetRandomPhoto(count: 3, query:"woman");

            var randomPhotoFeatured = await client.GetRandomPhoto(featured: true);
            var randomPortraitPhoto = await client.GetRandomPhoto(Orientation.Portrait);
            var randomPortraitPhotoFeatured = await client.GetRandomPhoto(Orientation.Portrait, featured: true);

            Assert.IsNotNull(randomPhoto);
            Assert.IsNotNull(randomPhotoFromCollection);
            Assert.IsNotNull(randomPhotoFromCollections);

            Assert.IsTrue(randomPhotoFromUser.Count > 0);
            Assert.IsTrue(randomPhotosFromQuery.Count > 0);
            Assert.IsTrue(randomPhotoFeatured.Count > 0);
            Assert.IsTrue(randomPortraitPhoto.Count > 0);
            Assert.IsTrue(randomPortraitPhotoFeatured.Count > 0);
        }

        [TestMethod]
        public async Task ListPhotosTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listPhotos = await client.ListPhotos();
            var listPhotosPaged = await client.ListPhotos(page:2, perPage:15, orderBy: OrderBy.Popular);

            Assert.IsTrue(listPhotos.Count > 0);
            Assert.IsTrue(listPhotosPaged.Count > 0);
        }

        [TestMethod]
        public async Task GetPhotoStatsTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listPhotos = await client.ListPhotos();
            var statsPhoto = await client.GetPhotoStats(listPhotos[0].Id);

            Assert.IsNotNull(statsPhoto);
        }

        [TestMethod]
        public async Task GetPhotoDownloadLinkTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listPhotos = await client.ListPhotos();
            var downloadLink = await client.GetPhotoDownloadLink(listPhotos[0].Id);

            Assert.IsNotNull(downloadLink);
        }

        [TestMethod]
        public async Task NotifyPropertyChangedTest() {
            var id = "TPv9dh822VA";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var photo = await client.GetPhoto(id);

            photo.Downloads = 20000;
        }
    }
}
