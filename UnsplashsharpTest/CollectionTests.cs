using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Unsplasharp;
using UnsplashsharpTest.Data;

namespace UnsplashsharpTest {
    [TestClass]
    public class CollectionTests {
        [TestMethod]
        public async Task GetCollectionTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listCollection = await client.ListCollections();
            var collection = await client.GetCollection(listCollection[0].Id);

            Assert.IsNotNull(collection);
        }

        [TestMethod]
        public async Task ListCollectionsTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listCollection = await client.ListCollections();
            var listCollectionPaged = await client.ListCollections(2);

            Assert.IsNotNull(listCollection);
            Assert.IsNotNull(listCollectionPaged);

            Assert.IsTrue(listCollection.Count > 0);
            Assert.IsTrue(listCollectionPaged.Count > 0);
        }

        [TestMethod]
        public async Task ListFeaturedCollectionsTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listFeaturedCollection = await client.ListFeaturedCollections();
            var listFeaturedCollectionPaged = await client.ListFeaturedCollections(2);

            Assert.IsNotNull(listFeaturedCollection);
            Assert.IsNotNull(listFeaturedCollectionPaged);

            Assert.IsTrue(listFeaturedCollection.Count > 0);
            Assert.IsTrue(listFeaturedCollectionPaged.Count > 0);
        }

        [TestMethod]
        public async Task ListCuratedCollectionsTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listCuratedCollection = await client.ListCuratedCollections();
            var listCuratedCollectionPaged = await client.ListCuratedCollections(2);

            Assert.IsNotNull(listCuratedCollection);
            Assert.IsNotNull(listCuratedCollectionPaged);

            Assert.IsTrue(listCuratedCollection.Count > 0);
            Assert.IsTrue(listCuratedCollectionPaged.Count > 0);
        }

        [TestMethod]
        public async Task GetCollectionPhotosTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listCollection = await client.ListCollections();
            var collection = await client.GetCollection(listCollection[0].Id);
            var listPhotos = await client.GetCollectionPhotos(collection.Id);

            Assert.IsNotNull(listPhotos);
            Assert.IsTrue(listPhotos.Count > 0);
        }

        public async Task ListRelatedCollectionsTest() {
            var client = new UnsplasharpClient(Credentials.ApplicationId);
            var listCollection = await client.ListCollections();
            var collectionsRelated = await client.ListRelatedCollections(listCollection[0].Id);

            Assert.IsNotNull(collectionsRelated);
            Assert.IsTrue(collectionsRelated.Count > 0);
        }
    }
}
