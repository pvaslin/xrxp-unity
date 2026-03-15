using System;

namespace XRXP.Recorder.Models
{
    public class InternalEvent : RecordBase
    {
        public long Time;
        public string Property;
        public string Value;
        public string InternalSystemId;

        public InternalEvent(long time, string property, Jsonable value, InternalSystem internalSystem, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Time = time;
            this.Property = property;
            this.Value = value.ToJSON();
            this.InternalSystemId = internalSystem.Id;
            this.Protocol = protocol.Length > 0 ? protocol : "InternalEvent";
        }
    }

}