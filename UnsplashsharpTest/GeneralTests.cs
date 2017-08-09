using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Unsplasharp;
using UnsplashsharpTest.Data;

namespace UnsplashsharpTest {
    [TestClass]
    public class GeneralTests {
        [TestMethod]
        public async Task RateLimitTest() {
            var client = new Client(Credentials.ApplicationId);
            var photosFound = await client.GetRandomPhoto();

            Assert.IsTrue(client.RateLimitRemaining < client.MaxRateLimit);
        }

        [TestMethod]
        public async Task GetTotalStatsTest() {
            var client = new Client(Credentials.ApplicationId);
            var totalStats = await client.GetTotalStats();

            Assert.IsNotNull(totalStats);
            Assert.IsTrue(totalStats.Photos > 0);
        }

        [TestMethod]
        public async Task GetMonthlyStatsTest() {
            var client = new Client(Credentials.ApplicationId);
            var monthlyStats = await client.GetMonthlyStats();

            Assert.IsNotNull(monthlyStats);
            Assert.IsTrue(monthlyStats.Views > 0);
        }
    }
}
