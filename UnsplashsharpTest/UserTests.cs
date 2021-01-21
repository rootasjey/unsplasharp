using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Unsplasharp;
using UnsplashsharpTest.Data;

namespace UnsplashsharpTest {
    [TestClass]
    public class UserTests {
        [TestMethod]
        public async Task GetUserTest() {
            var username = "unsplash";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var user = await client.GetUser(username);
            var userCustomProfileImage = client.GetUser("seteales", width: 100, height: 100);

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task ListUserCollectionsTest() {
            var username = "unsplash";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var userCollections = await client.ListUserCollections(username);

            Assert.IsNotNull(userCollections);
        }

        [TestMethod]
        public async Task ListUserPhotosTest() {
            var username = "matthew";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var userPhotos = await client.ListUserPhotos(username, perPage: 40);
            var userPhotosCustomParam = await client.ListUserPhotos(username, page: 2, perPage: 2, stats: true);

            Assert.IsNotNull(userPhotos);
            Assert.IsTrue(userPhotos.Count > 0);
        }

        [TestMethod]
        public async Task ListUserLikedPhotosTest() {
            var username = "matthew";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var userLikedPhotos = await client.ListUserLikedPhotos(username);

            Assert.IsNotNull(userLikedPhotos);
            Assert.IsTrue(userLikedPhotos.Count > 0);
        }

        [TestMethod]
        public async Task GetUserStatsTest() {
            var username = "matthew";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var userStats = await client.GetUserStats(username);

            Assert.IsNotNull(userStats);
        }
    }
}
