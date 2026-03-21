using System.Threading;
using XRXP.Recorder.Models;

namespace XRXP.Recorder.Storage
{
    public interface IDataStorage
    {
        /// <summary>
        /// Prepare the trace to be send (open File, open websocket etc.)
        /// </summary>
        /// <returns></returns>
        public void Open(CancellationToken cancellationToken);

        /// <summary>
        /// Return the number of data who remain to be store
        /// </summary>
        /// <returns></returns>
        public int RemainingDataCount();

        /// <summary>
        /// Indicate to dispose the stream open (close file, close websocket etc.)
        /// If its remain data to store, they will not store and lost
        /// </summary>
        /// <returns></returns>
        public void Dispose();

        /// <summary>
        /// Add a trace to store
        /// </summary>
        /// <param name="trace"></param>
        public void AsyncAdd(RecordBase trace);      
    }

}