namespace XRXP.Recorder.Storage
{
    public readonly struct SerializedRecord
    {
        public readonly string Id;
        public readonly string Json;
        public readonly string FilePath;

        public SerializedRecord(string id, string json, string filePath = null)
        {
            Id = id;
            Json = json;
            FilePath = filePath;
        }
    }
}
