using NUnit.Framework;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class StatisticTests
    {
        private User _testUser;
        private Session _testSession;

        [SetUp]
        public void Setup()
        {
            _testUser = new User("user-123");
            _testSession = new Session("exp-001", _testUser, 1234567890L);
        }

        [Test]
        public void Constructor_WithRequiredParams_CreatesStatistic()
        {
            var statistic = new Statistic("completion_time", "45000", "ms", _testSession);
            
            Assert.IsNotNull(statistic);
            Assert.AreEqual("completion_time", statistic.Name);
            Assert.AreEqual("45000", statistic.Value);
            Assert.AreEqual("ms", statistic.Parameters);
            Assert.AreEqual(_testSession.Id, statistic.SessionId);
            Assert.AreEqual("Session", statistic.StatType);
            Assert.AreEqual("Statistic", statistic.Protocol);
            Assert.IsNotNull(statistic.Id);
        }
    }
}