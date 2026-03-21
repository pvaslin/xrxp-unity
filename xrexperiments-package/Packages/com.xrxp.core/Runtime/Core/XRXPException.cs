using System;

namespace XRXP
{
    [Serializable]
    public class XRXPException : Exception
    {
        public XRXPException() { }
        public XRXPException(string message) : base(String.Format("XRXP : {0}", message)) { }
        public XRXPException(string message, Exception innerException) : base(String.Format("XRXP : {0}", message), innerException) { }
    }
}