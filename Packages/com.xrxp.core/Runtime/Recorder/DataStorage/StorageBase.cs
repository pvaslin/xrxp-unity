using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace XRXP.Recorder.Storage
{
    public abstract class StorageBase : IDataStorage
    {
        private readonly BlockingCollection<SerializedRecord> _queue;
        private Thread _consumerThread;
        private CancellationTokenSource _linkedCts;
        private bool _addingCompleted = false;

        protected CancellationToken CancellationToken => _linkedCts?.Token ?? CancellationToken.None;

        protected StorageBase()
        {
            _queue = new BlockingCollection<SerializedRecord>(new ConcurrentQueue<SerializedRecord>());
        }

        public void Open(CancellationToken cancellationToken)
        {
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _consumerThread = new Thread(ConsumerThreadEntry)
            {
                IsBackground = true,
                Name = GetType().Name
            };
            _consumerThread.Start();
        }

        private void ConsumerThreadEntry()
        {
            try
            {
                OnOpen();
                foreach (var record in _queue.GetConsumingEnumerable(_linkedCts.Token))
                {
                    try
                    {
                        ProcessRecord(record);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"XRXP.Recorder [{GetType().Name}]: Error processing record {record.Id}: {e.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on cancellation
            }
            catch (Exception e)
            {
                Debug.LogError($"XRXP.Recorder [{GetType().Name}]: Consumer thread error: {e.Message}");
            }
            finally
            {
                try
                {
                    OnClose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"XRXP.Recorder [{GetType().Name}]: Error during close: {e.Message}");
                }
            }
        }

        public void Add(SerializedRecord record)
        {
            if (!_addingCompleted)
            {
                _queue.Add(record);
            }
        }

        public void CompleteAdding()
        {
            if (!_addingCompleted)
            {
                _addingCompleted = true;
                _queue.CompleteAdding();
            }
        }

        public int RemainingDataCount()
        {
            return _queue.Count;
        }

        public void Dispose()
        {
            CompleteAdding();

            Debug.Log($"XRXP.Recorder: Waiting for {GetType().Name} to drain and stop...");

            if (_consumerThread != null && _consumerThread.IsAlive)
            {
                if (!_consumerThread.Join(TimeSpan.FromSeconds(10)))
                {
                    Debug.LogWarning($"XRXP.Recorder: {GetType().Name} did not stop within 10s, cancelling...");
                    _linkedCts?.Cancel();
                    if (!_consumerThread.Join(TimeSpan.FromSeconds(2)))
                    {
                        Debug.LogError($"XRXP.Recorder: {GetType().Name} thread did not terminate.");
                    }
                }
            }

            _linkedCts?.Dispose();
            _queue.Dispose();

            Debug.Log($"XRXP.Recorder: {GetType().Name} stopped.");
        }

        protected abstract void OnOpen();
        protected abstract void ProcessRecord(SerializedRecord record);
        protected abstract void OnClose();
    }
}
