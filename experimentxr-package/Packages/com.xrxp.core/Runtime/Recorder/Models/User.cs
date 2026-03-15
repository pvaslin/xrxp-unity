using System;
using System.Collections.Generic;
using System.Linq;

namespace XRXP.Recorder.Models
{
    public class User : RecordWithProperties
    {
        private List<UserProperty> _userProperties = new List<UserProperty>();

        public User(string id, string protocol = "")
        {
            this.Id = id.Length > 0 ? id : Guid.NewGuid().ToString();
            this.Protocol = protocol.Length > 0 ? protocol : "User";
        }

        public void AddUserProperty(string property, string value)
        {
            this._userProperties.Add(new UserProperty(property, value, this));
        }

        public void AddUserProperties(Dictionary<string,string> properties)
        {
            foreach (KeyValuePair<string,string> property in properties)
            {
                this.AddUserProperty(property.Key,property.Value);
            }
        }

        public List<UserProperty> GetUserProperties()
        {
            return this._userProperties;
        }

        public override List<RecordBase> GetProperties()
        {
            return new List<RecordBase>(this._userProperties);
        }
    }
}
