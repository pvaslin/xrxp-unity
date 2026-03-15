
using System;

namespace XRXP.Recorder.Models
{
    public enum SystemName
    {
        BodyTracking, ObjectTracking, EyeTracking
    }

    public enum SystemType // Enumerates the system-type define  
    {
        WorldPosition, QuantitativeValue
    }

    public class InternalSystem : RecordBase
    {
        public string Name;
        public string SystemTypeName;
        public string UserId;
        public string SessionId;
        private Session _session;
        private User _user;
        public InternalSystem(string systemName, SystemType systemType, User user, Session session, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Name = systemName;
            this.SystemTypeName = systemType.ToString("g"); // Get the name of the constant value
            this.UserId = user.Id;
            this._user = user;
            this._session = session;
            this.SessionId = session.Id;
            this.Protocol = protocol.Length > 0 ? protocol : "InternalSystem";
        }
    }

}