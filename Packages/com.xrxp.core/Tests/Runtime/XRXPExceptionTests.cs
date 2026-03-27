using System;
using NUnit.Framework;
using XRXP;

namespace XRXP.Tests
{
    [TestFixture]
    public class XRXPExceptionTests
    {
        [Test]
        public void Constructor_Default_CreatesException()
        {
            var exception = new XRXPException();
            
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<Exception>(exception);
        }

        [Test]
        public void Constructor_WithMessage_PrefixesXRXP()
        {
            var message = "Test error message";
            var exception = new XRXPException(message);
            
            Assert.IsTrue(exception.Message.Contains("XRXP :"));
            Assert.IsTrue(exception.Message.Contains(message));
        }

        [Test]
        public void Constructor_WithMessageAndInnerException_PreservesInnerException()
        {
            var innerMessage = "Inner exception";
            var outerMessage = "Outer exception";
            var innerException = new InvalidOperationException(innerMessage);
            
            var exception = new XRXPException(outerMessage, innerException);
            
            Assert.IsNotNull(exception.InnerException);
            Assert.AreEqual(innerException, exception.InnerException);
            Assert.IsTrue(exception.Message.Contains("XRXP :"));
            Assert.IsTrue(exception.Message.Contains(outerMessage));
        }
    }
}