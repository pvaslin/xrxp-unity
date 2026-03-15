using NUnit.Framework;
using System.Collections.Generic;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class QuestionTests
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
        public void Constructor_WithRequiredParams_CreatesQuestion()
        {
            var question = new Question("Comfort Level", "High", _testSession);
            
            Assert.IsNotNull(question);
            Assert.AreEqual("Comfort Level", question.Label);
            Assert.AreEqual("High", question.Answer);
            Assert.AreEqual(_testSession.Id, question.SessionId);
            Assert.AreEqual("Question", question.Protocol);
            Assert.IsNotNull(question.Id);
        }

        [Test]
        public void Constructor_WithCustomProtocol_UsesProvidedProtocol()
        {
            var question = new Question("Comfort Level", "High", _testSession, "CustomQuestion");
            
            Assert.AreEqual("CustomQuestion", question.Protocol);
        }

        [Test]
        public void UpdateAnswer_ChangesAnswer()
        {
            var question = new Question("Comfort Level", "High", _testSession);
            
            question.UpdateAnswer("Medium");
            
            Assert.AreEqual("Medium", question.Answer);
        }

        [Test]
        public void AddQuestionProperty_AddsSingleProperty()
        {
            var question = new Question("Comfort Level", "High", _testSession);
            
            question.AddQuestionProperty("category", "safety");
            
            var properties = question.GetQuestionProperties();
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual("category", properties[0].Name);
            Assert.AreEqual("safety", properties[0].Value);
        }

        [Test]
        public void AddQuestionProperties_AddsMultipleProperties()
        {
            var question = new Question("Comfort Level", "High", _testSession);
            var properties = new Dictionary<string, string>
            {
                { "category", "safety" },
                { "urgency", "low" }
            };
            
            question.AddQuestionProperties(properties);
            
            var questionProperties = question.GetQuestionProperties();
            Assert.AreEqual(2, questionProperties.Count);
        }

        [Test]
        public void GetProperties_ReturnsAllProperties()
        {
            var question = new Question("Comfort Level", "High", _testSession);
            question.AddQuestionProperty("key1", "value1");
            question.AddQuestionProperty("key2", "value2");
            
            var properties = question.GetProperties();
            
            Assert.AreEqual(2, properties.Count);
        }
    }
}