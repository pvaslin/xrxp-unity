using NUnit.Framework;
using System;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class SessionTests
    {
        private User _testUser;
        private Environment _testEnvironment;

        [SetUp]
        public void Setup()
        {
            _testUser = new User("user-123");
            _testEnvironment = new Environment("env-456");
        }

        [Test]
        public void Constructor_WithBasicParams_CreatesSession()
        {
            var session = new Session("exp-001", _testUser, 1234567890L);
            
            Assert.IsNotNull(session);
            Assert.AreEqual("exp-001", session.ExperimentId);
            Assert.AreEqual("user-123", session.UserId);
            Assert.AreEqual(1234567890L, session.StartDate);
            Assert.AreEqual(0, session.EndDate);
            Assert.AreEqual("Session", session.Protocol);
            Assert.IsNotNull(session.Id);
        }

        [Test]
        public void Constructor_WithComments_SetsComments()
        {
            var comments = "Test session comments";
            var session = new Session("exp-001", _testUser, 1234567890L, comments);
            
            Assert.AreEqual(comments, session.Comments);
        }

        [Test]
        public void Constructor_WithEnvironment_SetsEnvironmentId()
        {
            var session = new Session("exp-001", _testUser, 1234567890L, null, _testEnvironment, null);
            
            Assert.AreEqual("env-456", session.EnvironmentId);
        }

        [Test]
        public void Constructor_WithParentSession_SetsParentId()
        {
            var parentSession = new Session("exp-001", _testUser, 1234567890L);
            var childSession = new Session("exp-001", _testUser, 1234567891L, null, null, parentSession);
            
            Assert.AreEqual(parentSession.Id, childSession.Parent);
        }

        [Test]
        public void Constructor_WithCustomId_UsesProvidedId()
        {
            var customId = "session-789";
            var session = new Session(customId, "exp-001", _testUser, 1234567890L);
            
            Assert.AreEqual(customId, session.Id);
        }

        [Test]
        public void Constructor_WithNullId_GeneratesNewId()
        {
            var session = new Session(null, "exp-001", _testUser, 1234567890L);
            
            Assert.IsNotNull(session.Id);
            Assert.IsNotEmpty(session.Id);
        }

        [Test]
        public void UpdateEndDate_UpdatesEndDateAndProtocol()
        {
            var session = new Session("exp-001", _testUser, 1234567890L);
            var endDate = 1234567900L;
            
            session.UpdateEndDate(endDate);
            
            Assert.AreEqual(endDate, session.EndDate);
            Assert.AreEqual("Session_updateenddate", session.Protocol);
            Assert.IsFalse(session.isSent);
        }

        [Test]
        public void AddInternalSystem_AddsSystem()
        {
            var session = new Session("exp-001", _testUser, 1234567890L);
            var internalSystem = new InternalSystem("Tracker1", SystemType.WorldPosition, _testUser, session);
            
            session.AddInternalSystem(internalSystem);
            
            Assert.IsTrue(session.TryGetInternalSystem("Tracker1", out _));
        }

        [Test]
        public void TryGetInternalSystem_ExistingSystem_ReturnsTrue()
        {
            var session = new Session("exp-001", _testUser, 1234567890L);
            var internalSystem = new InternalSystem("Tracker1", SystemType.WorldPosition, _testUser, session);
            session.AddInternalSystem(internalSystem);
            
            var result = session.TryGetInternalSystem("Tracker1", out var retrievedSystem);
            
            Assert.IsTrue(result);
            Assert.IsNotNull(retrievedSystem);
            Assert.AreEqual("Tracker1", retrievedSystem.Name);
        }

        [Test]
        public void TryGetInternalSystem_NonExistingSystem_ReturnsFalse()
        {
            var session = new Session("exp-001", _testUser, 1234567890L);
            
            var result = session.TryGetInternalSystem("NonExistent", out var retrievedSystem);
            
            Assert.IsFalse(result);
            Assert.IsNull(retrievedSystem);
        }

        [Test]
        public void GetEnvironment_ReturnsEnvironment()
        {
            var session = new Session("exp-001", _testUser, 1234567890L, null, _testEnvironment, null);
            
            var env = session.GetEnvironment();
            
            Assert.IsNotNull(env);
            Assert.AreEqual("env-456", env.Id);
        }

        [Test]
        public void GetUserInformation_ReturnsUser()
        {
            var session = new Session("exp-001", _testUser, 1234567890L);
            
            var user = session.GetUserInformation();
            
            Assert.IsNotNull(user);
            Assert.AreEqual("user-123", user.Id);
        }
    }
}