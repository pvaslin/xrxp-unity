using System;
using System.Threading.Tasks;
using System.Net.WebSockets;
using XRXP.Recorder.Models;
using System.Threading;
using UnityEngine;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.Concurrent;
using UnityEngine.Profiling;

namespace XRXP.Recorder.Storage
{
    public class BackupStorage : IDataStorage
    {
        private ConcurrentQueue<RecordBase> _queue;
        private Task _task;
        private bool _running = false;
        private StreamWriter _stream;
        private string _directory;
        private string _path;
        private CancellationToken _cancellationToken;

        public BackupStorage(string directory)
        {
            this._directory = String.Format("{0}/backup", directory);
            this._queue = new ConcurrentQueue<RecordBase>();

        }

        public void OpenFile()
        {
            if (!Directory.Exists(this._directory))
            {
                try
                {
                    Directory.CreateDirectory(this._directory);
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            this._path = String.Format("{0}/Trace_{1}.backup.gz", this._directory, DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss"));
            try
            {
                this._stream = new StreamWriter(new GZipStream(File.CreateText(this._path).BaseStream, System.IO.Compression.CompressionLevel.Fastest));
                this._stream.AutoFlush = true;
                Debug.Log(this._path);
            }
            catch (System.Exception e)
            {
                throw new XRXPException($"XRXP.Trace : {e.Message}");
            }
        }

        public bool WriteTrace(RecordBase trace)
        {
            bool written = true;
            try
            {
                this._stream.WriteLine(JsonUtility.ToJson(trace));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"XRXP.Trace : {e.Message}");
                written = false;
            }
            if (trace is RecordWithProperties)
            {
                foreach (var property in ((RecordWithProperties)trace).GetProperties())
                {
                    written = written && this.WriteTrace(property);
                }
            }
            return written;
        }

        public bool DisposeFile()
        {
            try
            {
                this._stream.Dispose();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"XRXP.Trace : {e.Message}");
                return false;
            }
        }
        private async Task SendLoop()
        {
// #if UNITY_EDITOR
//             Profiler.BeginThreadProfiling("StorageSystem", this.GetType().ToString());
//             Debug.Log($"Task: Internal Loop of Backup Storage {System.Threading.Thread.CurrentThread.ManagedThreadId}");
// #endif
            this._running = true;
            this.OpenFile();
            while (this._running && ! this._cancellationToken.IsCancellationRequested)
            {
                RecordBase trace;
                if (this._queue.TryPeek(out trace))
                {
                    if (this.WriteTrace(trace))
                    {
                        this._queue.TryDequeue(out trace);
                    }
                    else
                    {
                        throw new XRXPException($"File not accessible : {this._path}");
                    }
                }
                else
                {
                    this._stream.Flush();
                    await Task.Delay(10,this._cancellationToken);
                }
            }
            this.DisposeFile();
// #if UNITY_EDITOR
//             Profiler.EndThreadProfiling();
// #endif
        }

        public void Open(CancellationToken cancellationToken)
        {
            this._cancellationToken = cancellationToken;
            TaskFactory taskFactory = new TaskFactory(cancellationToken, TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning, TaskScheduler.Default);
            this._task = taskFactory.StartNew(this.SendLoop);
        }

        public void Dispose()
        {
            this._running = false;
            Debug.Log($"XRXP.Recorder: Wait for backup storage to end");
            this._task.Wait();
            this._task.Dispose();
            Debug.Log($"XRXP.Recorder: Backup storage stopped");

        }

        public void AsyncAdd(RecordBase trace)
        {
            this._queue.Enqueue(trace);
        }

        public int RemainingDataCount()
        {
            return this._queue.Count;
        }
    }
}
