using System;

namespace XRXP.Recorder.Models
{
    [System.Serializable]
    public class EnvironmentProperty : RecordBase
    { 
        public string Property;
        public string Value;
        public string EnvironmentId;

        private Environment _environment;

        public EnvironmentProperty(string property, string value, Environment environment, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Property = property;
            this.Value = value;
            this._environment = environment;
            this.EnvironmentId = environment.Id;
            this.Protocol = protocol.Length > 0 ? protocol : "EnvironmentProperty";

        }
    }
}
