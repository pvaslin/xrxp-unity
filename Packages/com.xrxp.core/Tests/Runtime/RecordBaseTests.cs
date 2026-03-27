using NUnit.Framework;
using XRXP.Recorder.Models;

namespace XRXP.Tests
{
    [TestFixture]
    public class RecordBaseTests
    {
        private class TestRecord : RecordBase
        {
        }

        [Test]
        public void RecordBase_DefaultValues_AreSet()
        {
            var record = new TestRecord();
            
            Assert.IsNotNull(record.Protocol);
            Assert.AreEqual(string.Empty, record.Protocol);
            Assert.IsFalse(record.isSent);
        }

        [Test]
        public void RecordBase_CanSetProperties()
        {
            var record = new TestRecord
            {
                Protocol = "TestProtocol",
                isSent = true,
                Id = "test-id-123"
            };
            
            Assert.AreEqual("TestProtocol", record.Protocol);
            Assert.IsTrue(record.isSent);
            Assert.AreEqual("test-id-123", record.Id);
        }
    }

    [TestFixture]
    public class RecordWithPropertiesTests
    {
        private class TestRecordWithProperties : RecordWithProperties
        {
            public override System.Collections.Generic.List<RecordBase> GetProperties()
            {
                return new System.Collections.Generic.List<RecordBase>();
            }
        }

        [Test]
        public void RecordWithProperties_IsRecordBase()
        {
            var record = new TestRecordWithProperties();
            
            Assert.IsInstanceOf<RecordBase>(record);
        }

        [Test]
        public void RecordWithProperties_GetProperties_ReturnsList()
        {
            var record = new TestRecordWithProperties();
            
            var properties = record.GetProperties();
            
            Assert.IsNotNull(properties);
        }
    }
}