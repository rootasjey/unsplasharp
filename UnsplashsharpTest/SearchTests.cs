using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Unsplasharp;
using UnsplashsharpTest.Data;

namespace UnsplashsharpTest {
    [TestClass]
    public class SearchTests {
        [TestMethod]
        public async Task SearchPhotosTestLowSafety()
        {
            await SearchPhotosTest(UnsplasharpClient.ContentSafety.Low);
        }

        [TestMethod]
        public async Task SearchPhotosTestHighSafety()
        {
            await SearchPhotosTest(UnsplasharpClient.ContentSafety.High);
        }

        private async Task SearchPhotosTest(UnsplasharpClient.ContentSafety contentFilter) {
            var query = "mountains";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            client.ContentFilter = contentFilter;
            var photosFound = await client.SearchPhotos(query);
            var photosFoundPaged = await client.SearchPhotos(query, 2);

            Assert.IsTrue(photosFound.Count > 0);
            Assert.IsTrue(photosFoundPaged.Count > 0);

            Assert.IsNotNull(client.LastPhotosSearchQuery);
            Assert.IsTrue(client.LastPhotosSearchTotalPages > 0);
            Assert.IsTrue(client.LastPhotosSearchTotalResults > 0);
        }

        [TestMethod]
        public async Task SearchCollectionsTest() {
            var query = "mountains";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var collectionsFound = await client.SearchCollections(query);
            var collectionsFoundPaged = await client.SearchCollections(query, 2);

            Assert.IsTrue(collectionsFound.Count > 0);
            Assert.IsTrue(collectionsFoundPaged.Count > 0);

            Assert.IsNotNull(client.LastCollectionsSearchQuery);
            Assert.IsTrue(client.LastCollectionsSearchTotalResults > 0);
            Assert.IsTrue(client.LastCollectionsSearchTotalPages > 0);
        }

        [TestMethod]
        public async Task SearchUsersTest() {
            var query = "mountains";
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var usersFound = await client.SearchUsers(query);
            var usersFoundPaged = await client.SearchUsers(query, 2);

            Assert.IsTrue(usersFound.Count > 0);
            Assert.IsTrue(usersFoundPaged.Count > 0);

            Assert.IsNotNull(client.LastUsersSearchQuery);
            Assert.IsTrue(client.LastUsersSearchTotalResults > 0);
            Assert.IsTrue(client.LastUsersSearchTotalPages > 0);
        }
    }
}
