using System;
using System.Threading.Tasks;
using System.Net.WebSockets;
using XRXP.Recorder.Models;
using System.Threading;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using UnityEngine.Profiling;

namespace XRXP.Recorder.Storage
{
    public class RemoteStorage : IDataStorage
    {
        private ConcurrentQueue<RecordBase> _queue;
        private Task _task;
        private bool _running = false;
        private const int _maxAttempt = 3;
        private ClientWebSocket _websocket;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Uri _uri;
        private string _authToken = null;

        public RemoteStorage(Uri uri, string authorizationToken = null)
        {
            this._uri = uri;
            this._authToken = authorizationToken;
            this._cancellationTokenSource = new CancellationTokenSource();
            this._queue = new ConcurrentQueue<RecordBase>();
        }
        private async Task<bool> OpenWebsocket()
        {
            this._websocket = new ClientWebSocket();
            if (this._authToken != null)
            {
                this._websocket.Options.SetRequestHeader("Authorization", "Basic "+ this._authToken);
            }
            this._websocket.Options.KeepAliveInterval = new TimeSpan(0, 0, 1);
            int attempts = _maxAttempt;
            while (attempts > 0 && (this._websocket.State != WebSocketState.Open))
            {
                try
                {
                    await this._websocket.ConnectAsync(this._uri, this._cancellationToken);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"XRXP.Recorder: {exception.Message}");
                    attempts -= 1;
                    await Task.Delay(100);
                }
            }
            return this._websocket.State == WebSocketState.Open;
        }

        private async Task<bool> DisposeWebSocket()
        {
            try
            {
                await this._websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", this._cancellationToken);
            }
            catch (Exception exception)
            {
                Debug.LogError($"XRXP.Recorder: {exception.Message}");
            }
            return this._websocket.State == WebSocketState.Closed;
        }

        private async Task<bool> SendData(string data, string idData)
        {
            if (this._websocket == null)
            {
                throw new Exception("The websocket is not open, please use Open() before sending data");
            }
            if (this._websocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
                ArraySegment<byte> bytesBuffer = new ArraySegment<byte>(new byte[2048]);
                try
                {
                    await this._websocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, this._cancellationToken);
                    // CRITICAL not check if is correctly handle by the server
                    // await this._websocket.ReceiveAsync(bytesBuffer, this._cancellationTokenSource.Token);
                    // return Encoding.UTF8.GetString(bytesBuffer).Contains(idData);
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogError($"XRXP.Recorder: {exception.Message}");
                }
            }
            return false;
        }

        private async Task<bool> SendTrace(RecordBase trace)
        {
            bool sent = true;
            if (!trace.isSent)
            {
                sent = await this.SendData(JsonUtility.ToJson(trace), trace.Id);
            }
            if (trace is RecordWithProperties)
            {
                foreach (var property in ((RecordWithProperties)trace).GetProperties())
                {
                    if (!property.isSent)
                    {
                        sent = sent && await this.SendTrace(property);
                    }
                }
            }
            return sent;
        }

        private async Task SendLoop()
        {
// #if UNITY_EDITOR
//             Profiler.BeginThreadProfiling("StorageSystem", this.GetType().ToString());
//             Debug.Log($"Task: Internal Loop of Remote Storage {System.Threading.Thread.CurrentThread.ManagedThreadId}");
// #endif
            this._running = true;
            await this.OpenWebsocket();
            while (this._running  && ! this._cancellationToken.IsCancellationRequested)
            {
                RecordBase trace;
                if (this._queue.TryPeek(out trace))
                {
                    if (await this.SendTrace(trace))
                    {
                        this._queue.TryDequeue(out trace);
                    }
                    else
                    {
                        if (!await this.OpenWebsocket())
                        {
                            Debug.LogWarning($"Server not accessible : {this._uri.Host}");
                            await Task.Delay(1000);
                        }
                    }
                }
                else
                {
                    await Task.Delay(10, this._cancellationToken);
                }
            }
            this._websocket.Dispose();
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
            Debug.Log($"XRXP.Recorder: Wait for remote storage to end");
            if (!this._task.Wait(TimeSpan.FromSeconds(5)))
            {
                Debug.LogWarning("XRXP.Recorder: Remote storage did not stop within timeout.");
            }
            this._task.Dispose();
            Debug.Log($"XRXP.Recorder: Remote storage stopped");
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
