using System;

namespace XRXP.Recorder.Models
{
    [Serializable]
    class RecordException : Exception
    {
        public RecordException() { }
        public RecordException(string message) : base(String.Format("XRXP.Trace : {0}")) { }
        public RecordException(string message, Exception innerException) : base(String.Format("XRXP.Trace : {0}"), innerException) { }        
    }
}