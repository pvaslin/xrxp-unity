using NUnit.Framework;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class MediaEventTests
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
        public void Constructor_WithRequiredParams_CreatesMediaEvent()
        {
            var mediaEvent = new MediaEvent(1234567890L, "image/png", "screenshot", _testUser, _testSession);
            
            Assert.IsNotNull(mediaEvent);
            Assert.AreEqual(1234567890L, mediaEvent.Time);
            Assert.AreEqual("image/png", mediaEvent.MimeType);
            Assert.AreEqual("screenshot", mediaEvent.Name);
            Assert.AreEqual("user-123", mediaEvent.UserId);
            Assert.AreEqual(_testSession.Id, mediaEvent.SessionId);
            Assert.AreEqual("MediaEvent", mediaEvent.Protocol);
            Assert.AreEqual(0, mediaEvent.Duration);
            Assert.IsNotNull(mediaEvent.Id);
        }

        [Test]
        public void Constructor_WithDuration_SetsDuration()
        {
            var duration = 5000;
            var mediaEvent = new MediaEvent(1234567890L, "video/mp4", "recording", _testUser, _testSession, duration);
            
            Assert.AreEqual(duration, mediaEvent.Duration);
        }

        [Test]
        public void Constructor_WithCustomProtocol_UsesProvidedProtocol()
        {
            var mediaEvent = new MediaEvent(1234567890L, "image/png", "screenshot", _testUser, _testSession, 0, "CustomMedia");
            
            Assert.AreEqual("CustomMedia", mediaEvent.Protocol);
        }

        [Test]
        public void SetFilePath_SetsFilePath()
        {
            var mediaEvent = new MediaEvent(1234567890L, "image/png", "screenshot", _testUser, _testSession);
            var filePath = "/path/to/file.png";
            
            mediaEvent.SetFilePath(filePath);
            
            Assert.AreEqual(filePath, mediaEvent.GetFilePath());
        }

        [Test]
        public void GetFilePath_BeforeSetting_ReturnsNull()
        {
            var mediaEvent = new MediaEvent(1234567890L, "image/png", "screenshot", _testUser, _testSession);
            
            var filePath = mediaEvent.GetFilePath();
            
            Assert.IsNull(filePath);
        }
    }
}