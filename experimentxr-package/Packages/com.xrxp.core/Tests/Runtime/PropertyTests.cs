using NUnit.Framework;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class UserPropertyTests
    {
        private User _testUser;

        [SetUp]
        public void Setup()
        {
            _testUser = new User("user-123");
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var property = new UserProperty("age", "25", _testUser);
            
            Assert.AreEqual("age", property.Property);
            Assert.AreEqual("25", property.Value);
            Assert.AreEqual("user-123", property.UserId);
            Assert.AreEqual("UserProperty", property.Protocol);
        }

        [Test]
        public void UpdateValue_ChangesValue()
        {
            var property = new UserProperty("age", "25", _testUser);
            
            property.UpdateValue("26");
            
            Assert.AreEqual("26", property.Value);
        }
    }

    [TestFixture]
    public class EnvironmentPropertyTests
    {
        private Environment _testEnvironment;

        [SetUp]
        public void Setup()
        {
            _testEnvironment = new Environment("env-456");
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var property = new EnvironmentProperty("room", "lab-1", _testEnvironment);
            
            Assert.AreEqual("room", property.Name);
            Assert.AreEqual("lab-1", property.Value);
            Assert.AreEqual("env-456", property.EnvironmentId);
            Assert.AreEqual("EnvironmentProperty", property.Protocol);
        }
    }

    [TestFixture]
    public class LogEventPropertyTests
    {
        private User _testUser;
        private Session _testSession;
        private LogEvent _testLogEvent;

        [SetUp]
        public void Setup()
        {
            _testUser = new User("user-123");
            _testSession = new Session("exp-001", _testUser, 1234567890L);
            _testLogEvent = new LogEvent(1234567890L, "User", "clicked", "Button", _testUser, _testSession);
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var property = new LogEventProperty("hand", "right", _testLogEvent);
            
            Assert.AreEqual("hand", property.Name);
            Assert.AreEqual("right", property.Value);
            Assert.AreEqual(_testLogEvent.Id, property.LogEventId);
            Assert.AreEqual("LogEventProperty", property.Protocol);
        }
    }

    [TestFixture]
    public class QuestionPropertyTests
    {
        private User _testUser;
        private Session _testSession;
        private Question _testQuestion;

        [SetUp]
        public void Setup()
        {
            _testUser = new User("user-123");
            _testSession = new Session("exp-001", _testUser, 1234567890L);
            _testQuestion = new Question("Comfort", "High", _testSession);
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            var property = new QuestionProperty("category", "safety", _testQuestion);
            
            Assert.AreEqual("category", property.Name);
            Assert.AreEqual("safety", property.Value);
            Assert.AreEqual(_testQuestion.Id, property.QuestionId);
            Assert.AreEqual("QuestionProperty", property.Protocol);
        }
    }
}