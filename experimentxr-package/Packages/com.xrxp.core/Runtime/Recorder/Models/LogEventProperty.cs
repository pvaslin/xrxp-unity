using System;

namespace XRXP.Recorder.Models
{
    public class LogEventProperty : RecordBase
    {
        public string Property;
        public string Value;
        public string LogEventId;
        private LogEvent _logEvent;

        public LogEventProperty(string property, string value, LogEvent logEvent, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Property = property;
            this.Value = value;
            this._logEvent = logEvent;
            this.LogEventId = logEvent.Id;
            this.Protocol = protocol.Length > 0 ? protocol : "LogEventProperty";
        }
    }
}
