using NUnit.Framework;
using System.Collections.Generic;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class LogEventTests
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
        public void Constructor_WithRequiredParams_CreatesLogEvent()
        {
            var logEvent = new LogEvent(1234567890L, "User", "clicked", "Button_A", _testUser, _testSession);
            
            Assert.IsNotNull(logEvent);
            Assert.AreEqual(1234567890L, logEvent.Time);
            Assert.AreEqual("User", logEvent.Actor);
            Assert.AreEqual("clicked", logEvent.Verb);
            Assert.AreEqual("Button_A", logEvent.Object);
            Assert.AreEqual("user-123", logEvent.UserId);
            Assert.AreEqual(_testSession.Id, logEvent.SessionId);
            Assert.AreEqual("LogEvent", logEvent.Protocol);
            Assert.IsNotNull(logEvent.Id);
        }

        [Test]
        public void Constructor_WithDuration_SetsDuration()
        {
            var duration = 1500;
            var logEvent = new LogEvent(1234567890L, "User", "clicked", "Button_A", _testUser, _testSession, duration);
            
            Assert.AreEqual(duration, logEvent.Duration);
        }

        [Test]
        public void Constructor_WithCustomProtocol_UsesProvidedProtocol()
        {
            var logEvent = new LogEvent(1234567890L, "User", "clicked", "Button_A", _testUser, _testSession, null, "CustomLogEvent");
            
            Assert.AreEqual("CustomLogEvent", logEvent.Protocol);
        }

        [Test]
        public void AddEventProperty_AddsSingleProperty()
        {
            var logEvent = new LogEvent(1234567890L, "User", "clicked", "Button_A", _testUser, _testSession);
            
            logEvent.AddEventProperty("position", "(10, 20, 30)");
            
            var properties = logEvent.GetLogEventProperties();
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual("position", properties[0].Name);
            Assert.AreEqual("(10, 20, 30)", properties[0].Value);
        }

        [Test]
        public void AddEventProperties_AddsMultipleProperties()
        {
            var logEvent = new LogEvent(1234567890L, "User", "clicked", "Button_A", _testUser, _testSession);
            var properties = new Dictionary<string, string>
            {
                { "position", "(10, 20, 30)" },
                { "hand", "right" }
            };
            
            logEvent.AddEventProperties(properties);
            
            var logProperties = logEvent.GetLogEventProperties();
            Assert.AreEqual(2, logProperties.Count);
        }

        [Test]
        public void GetProperties_ReturnsAllProperties()
        {
            var logEvent = new LogEvent(1234567890L, "User", "clicked", "Button_A", _testUser, _testSession);
            logEvent.AddEventProperty("key1", "value1");
            logEvent.AddEventProperty("key2", "value2");
            
            var properties = logEvent.GetProperties();
            
            Assert.AreEqual(2, properties.Count);
        }
    }
}