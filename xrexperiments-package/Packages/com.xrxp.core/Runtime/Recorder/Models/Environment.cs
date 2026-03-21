using System;
using System.Collections.Generic;

namespace XRXP.Recorder.Models
{
    public class Environment : RecordWithProperties
    {
        private List<EnvironmentProperty> _environmentProperties = new List<EnvironmentProperty>();
        public Environment() : this(Guid.NewGuid().ToString()) { }
        public Environment(string id, string protocol = "")
        {
            this.Id = id;
            this.Protocol = protocol.Length > 0 ? protocol : "Environment";
        }

        public void AddEnvironmentProperty(string property, string value)
        {
            EnvironmentProperty environmentProperty = new EnvironmentProperty(property, value, this);
            this._environmentProperties.Add(environmentProperty);
        }

        public void AddEnvironmentProperties(Dictionary<string,string> properties)
        {
            foreach (KeyValuePair<string,string> item in properties)
            {
                this.AddEnvironmentProperty(item.Key,item.Value);
            }
        }

        public List<EnvironmentProperty> GetEnvironmentProperties()
        {
            return this._environmentProperties;
        }
        public override List<RecordBase> GetProperties()
        {
            return new List<RecordBase>(this._environmentProperties);
        }
    }
}