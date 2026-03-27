using System;

namespace XRXP.Recorder.Models
{
    internal class Statistic : RecordBase
    {
        public string Name;
        public string Value;
        public string Parameters;
        public string StatType;
        public string SessionId = "";
        private Session _session;

        /// <summary>
        /// Create a Session statistic
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="session"></param>
        public Statistic(string name, string value, string parameters, Session session)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Name = name;
            this.Value = value;
            this.Parameters = parameters;
            this._session = session;
            this.SessionId = session.Id;
            this.StatType = "Session";
            this.Protocol = "Statistic";
        }
    }

}
