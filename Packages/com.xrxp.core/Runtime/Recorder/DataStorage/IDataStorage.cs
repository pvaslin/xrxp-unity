using System;
using System.Threading;

namespace XRXP.Recorder.Storage
{
    public interface IDataStorage : IDisposable
    {
        /// <summary>
        /// Start the storage backend (open connections, start consumer thread).
        /// </summary>
        void Open(CancellationToken cancellationToken);

        /// <summary>
        /// Return the number of records remaining to be stored.
        /// </summary>
        int RemainingDataCount();

        /// <summary>
        /// Signal that no more records will be added. The consumer thread will
        /// drain remaining records and exit.
        /// </summary>
        void CompleteAdding();

        /// <summary>
        /// Enqueue a pre-serialized record for storage.
        /// </summary>
        void Add(SerializedRecord record);
    }
}
