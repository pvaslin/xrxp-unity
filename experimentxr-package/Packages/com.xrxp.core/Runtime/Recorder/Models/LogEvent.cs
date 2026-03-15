using System;
using System.Collections.Generic;

using UnityEditor;

namespace XRXP.Recorder.Models
{
    public class LogEvent : RecordWithProperties
    {
        public long Time;
        public int? Duration; // in ms 
        public string Actor;
        public string Verb;
        public string Object;
        public string UserId;
        public string SessionId;
        private List<LogEventProperty> _logEventProperties = new List<LogEventProperty>();
        private Session _session;
        private User _user;

        public LogEvent(long time, string actor, string verb, string @object, User user, Session session, int? duration = null, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Time = time;
            this.Duration = duration;
            this.Actor = actor;
            this.Verb = verb;
            this.Object = @object;
            this._session = session;
            this.SessionId = session.Id;
            this._user = user;
            this.UserId = user.Id;
            this.Protocol = protocol.Length > 0 ? protocol : "LogEvent";
        }

        public void AddEventProperty(string property, string value)
        {
            this._logEventProperties.Add(new LogEventProperty(property, value, this));
        }

        public void AddEventProperties(Dictionary<string,string> properties)
        {
            foreach (KeyValuePair<string,string> item in properties)
            {
                this.AddEventProperty(item.Key,item.Value);
            }
        }

        public List<LogEventProperty> GetLogEventProperties()
        {
            return this._logEventProperties;
        }
        public override List<RecordBase> GetProperties()
        {
            return new List<RecordBase>(this._logEventProperties); 
        }
    }
}