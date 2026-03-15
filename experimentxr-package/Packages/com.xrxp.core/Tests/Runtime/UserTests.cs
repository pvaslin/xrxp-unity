using NUnit.Framework;
using System.Collections.Generic;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void Constructor_WithEmptyId_GeneratesGuid()
        {
            var user = new User("");
            
            Assert.IsNotNull(user.Id);
            Assert.IsNotEmpty(user.Id);
            Assert.AreEqual("User", user.Protocol);
        }

        [Test]
        public void Constructor_WithCustomId_UsesProvidedId()
        {
            var customId = "user-123";
            var user = new User(customId);
            
            Assert.AreEqual(customId, user.Id);
        }

        [Test]
        public void Constructor_WithCustomProtocol_UsesProvidedProtocol()
        {
            var user = new User("test-id", "CustomUser");
            
            Assert.AreEqual("CustomUser", user.Protocol);
        }

        [Test]
        public void AddUserProperty_AddsSingleProperty()
        {
            var user = new User("test-id");
            
            user.AddUserProperty("age", "25");
            
            var properties = user.GetUserProperties();
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual("age", properties[0].Name);
            Assert.AreEqual("25", properties[0].Value);
        }

        [Test]
        public void AddUserProperties_AddsMultipleProperties()
        {
            var user = new User("test-id");
            var properties = new Dictionary<string, string>
            {
                { "age", "25" },
                { "gender", "female" },
                { "experience", "beginner" }
            };
            
            user.AddUserProperties(properties);
            
            var userProperties = user.GetUserProperties();
            Assert.AreEqual(3, userProperties.Count);
        }

        [Test]
        public void GetProperties_ReturnsAllProperties()
        {
            var user = new User("test-id");
            user.AddUserProperty("key1", "value1");
            user.AddUserProperty("key2", "value2");
            
            var properties = user.GetProperties();
            
            Assert.AreEqual(2, properties.Count);
        }
    }
}