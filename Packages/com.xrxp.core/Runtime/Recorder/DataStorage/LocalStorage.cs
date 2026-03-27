
using System.Threading;
using System.Threading.Tasks;
using XRXP.Recorder.Models;

namespace XRXP.Recorder.Storage
{
    public class LocalStorage : IDataStorage
    {

        public LocalStorage(string path, Session session)
        {
            throw new System.NotImplementedException();
        }

        public void AsyncAdd(RecordBase trace)
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