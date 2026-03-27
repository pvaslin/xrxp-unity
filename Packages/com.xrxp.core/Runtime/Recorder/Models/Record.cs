using System;
using System.Collections.Generic;

namespace XRXP.Recorder.Models
{
    [Serializable]
    public abstract class RecordBase
    {
        public string Protocol = string.Empty;
        public bool isSent = false;
        public string Id;
    }

    [Serializable]
    public abstract class RecordWithProperties : RecordBase
    {
        public abstract List<RecordBase> GetProperties();
    }
}
