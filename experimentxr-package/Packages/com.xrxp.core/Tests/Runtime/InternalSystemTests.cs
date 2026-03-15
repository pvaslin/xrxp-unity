using NUnit.Framework;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class InternalSystemTests
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
        public void Constructor_WithRequiredParams_CreatesInternalSystem()
        {
            var system = new InternalSystem("HeadTracker", SystemType.WorldPosition, _testUser, _testSession);
            
            Assert.IsNotNull(system);
            Assert.AreEqual("HeadTracker", system.Name);
            Assert.AreEqual("WorldPosition", system.SystemTypeName);
            Assert.AreEqual("user-123", system.UserId);
            Assert.AreEqual(_testSession.Id, system.SessionId);
            Assert.AreEqual("InternalSystem", system.Protocol);
            Assert.IsNotNull(system.Id);
        }

        [Test]
        public void Constructor_WithCustomProtocol_UsesProvidedProtocol()
        {
            var system = new InternalSystem("HeadTracker", SystemType.WorldPosition, _testUser, _testSession, "CustomSystem");
            
            Assert.AreEqual("CustomSystem", system.Protocol);
        }

        [Test]
        public void Constructor_SystemType_IsSerializedToString()
        {
            var system = new InternalSystem("Tracker", SystemType.QuantitativeValue, _testUser, _testSession);
            
            Assert.AreEqual("QuantitativeValue", system.SystemTypeName);
        }
    }

    [TestFixture]
    public class InternalEventTests
    {
        private User _testUser;
        private Session _testSession;
        private InternalSystem _testSystem;

        [SetUp]
        public void Setup()
        {
            _testUser = new User("user-123");
            _testSession = new Session("exp-001", _testUser, 1234567890L);
            _testSystem = new InternalSystem("HeadTracker", SystemType.WorldPosition, _testUser, _testSession);
        }

        [Test]
        public void Constructor_WithRequiredParams_CreatesInternalEvent()
        {
            var position = new WorldPosition(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
            var internalEvent = new InternalEvent(1234567890L, "position", position, _testSystem);
            
            Assert.IsNotNull(internalEvent);
            Assert.AreEqual(1234567890L, internalEvent.Time);
            Assert.AreEqual("position", internalEvent.Property);
            Assert.AreEqual(_testSystem.Id, internalEvent.InternalSystemId);
            Assert.AreEqual("InternalEvent", internalEvent.Protocol);
            Assert.IsNotNull(internalEvent.Id);
            Assert.IsNotNull(internalEvent.Value);
        }

        [Test]
        public void Constructor_WithCustomProtocol_UsesProvidedProtocol()
        {
            var position = new WorldPosition(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
            var internalEvent = new InternalEvent(1234567890L, "position", position, _testSystem, "CustomEvent");
            
            Assert.AreEqual("CustomEvent", internalEvent.Protocol);
        }

        [Test]
        public void Constructor_ValueIsSerializedToJSON()
        {
            var position = new WorldPosition(new UnityEngine.Vector3(1, 2, 3), UnityEngine.Quaternion.identity);
            var internalEvent = new InternalEvent(1234567890L, "position", position, _testSystem);
            
            Assert.IsFalse(string.IsNullOrEmpty(internalEvent.Value));
            Assert.IsTrue(internalEvent.Value.Contains("1"));
            Assert.IsTrue(internalEvent.Value.Contains("2"));
            Assert.IsTrue(internalEvent.Value.Contains("3"));
        }
    }
}