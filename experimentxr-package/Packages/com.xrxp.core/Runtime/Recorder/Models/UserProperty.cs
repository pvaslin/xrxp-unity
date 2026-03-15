using System;

namespace XRXP.Recorder.Models
{
    public class UserProperty : RecordBase
    {
        public string Property;
        public string Value;
        public string UserId;
        
        private User _user;

        public UserProperty(string property, string value, User user, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Property = property;
            this.Value = value;
            this._user = user;
            this.UserId = user.Id;
            this.Protocol = protocol.Length > 0 ? protocol : "UserProperty";
        }

        public void UpdateValue(string value)
        {
            this.Value = value;
        }
    }
}
