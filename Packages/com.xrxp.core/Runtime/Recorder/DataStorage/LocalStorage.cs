
using System.Threading;

namespace XRXP.Recorder.Storage
{
    public class LocalStorage : IDataStorage
    {
        public LocalStorage(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Add(SerializedRecord record)
        {
            throw new System.NotImplementedException();
        }

        public void CompleteAdding()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Open(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public int RemainingDataCount()
        {
            throw new System.NotImplementedException();
        }
    }
}
