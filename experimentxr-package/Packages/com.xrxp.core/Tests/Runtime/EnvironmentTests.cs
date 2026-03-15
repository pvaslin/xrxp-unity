using NUnit.Framework;
using System.Collections.Generic;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class EnvironmentTests
    {
        [Test]
        public void Constructor_Default_GeneratesGuid()
        {
            var env = new Environment();
            
            Assert.IsNotNull(env.Id);
            Assert.IsNotEmpty(env.Id);
            Assert.AreEqual("Environment", env.Protocol);
        }

        [Test]
        public void Constructor_WithCustomId_UsesProvidedId()
        {
            var customId = "env-456";
            var env = new Environment(customId);
            
            Assert.AreEqual(customId, env.Id);
        }

        [Test]
        public void Constructor_WithCustomProtocol_UsesProvidedProtocol()
        {
            var env = new Environment("test-id", "CustomEnvironment");
            
            Assert.AreEqual("CustomEnvironment", env.Protocol);
        }

        [Test]
        public void AddEnvironmentProperty_AddsSingleProperty()
        {
            var env = new Environment("test-env");
            
            env.AddEnvironmentProperty("room", "lab-1");
            
            var properties = env.GetEnvironmentProperties();
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual("room", properties[0].Name);
            Assert.AreEqual("lab-1", properties[0].Value);
        }

        [Test]
        public void AddEnvironmentProperties_AddsMultipleProperties()
        {
            var env = new Environment("test-env");
            var properties = new Dictionary<string, string>
            {
                { "room", "lab-1" },
                { "lighting", "dim" },
                { "temperature", "22C" }
            };
            
            env.AddEnvironmentProperties(properties);
            
            var envProperties = env.GetEnvironmentProperties();
            Assert.AreEqual(3, envProperties.Count);
        }

        [Test]
        public void GetProperties_ReturnsAllProperties()
        {
            var env = new Environment("test-env");
            env.AddEnvironmentProperty("key1", "value1");
            env.AddEnvironmentProperty("key2", "value2");
            
            var properties = env.GetProperties();
            
            Assert.AreEqual(2, properties.Count);
        }
    }
}