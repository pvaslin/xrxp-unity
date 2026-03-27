using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.WebSockets;
using System;
using XRXP.Recorder.Models;
using System.Collections.Concurrent;
using UnityEngine.Profiling;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace XRXP.Recorder.Storage
{
    public class FileSender
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private ConcurrentQueue<string> _queue;
        private Task _task;
        private bool _running = false;
        private Uri _endpoint;
        private string _authToken = null;

        public FileSender(Uri fileServer, string authorizationToken = null)
        {
            this._endpoint = fileServer;
            this._queue = new ConcurrentQueue<string>();
            this._authToken = authorizationToken;
        }
        private async Task SendLoop()
        {
#if UNITY_EDITOR
            Profiler.BeginThreadProfiling("StorageSystem", this.GetType().ToString());
            Debug.Log($"Task: Internal Loop of File sender {System.Threading.Thread.CurrentThread.ManagedThreadId}");
#endif
            this._running = true;
            while (this._running)
            {
                string filePath;
                if (this._queue.TryPeek(out filePath))
                {
                    if (await this.SendFile(filePath))
                    {
                        this._queue.TryDequeue(out filePath);
                    }
                    else
                    {
                        Debug.LogWarning($"Server not accessible : {this._endpoint.Host}");
                        await Task.Delay(5000);
                    }
                }
                else
                {
                    await Task.Yield();
                }
            }
#if UNITY_EDITOR
            Profiler.EndThreadProfiling();
#endif
        }
        private async Task<bool> SendFile(string path)
        {
            FileStream stream;
            try
            {
                stream = System.IO.File.OpenRead(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"XRXP.Trace.FileSender : Cannot open file {path} - {e.Message}");
                return false;
            }

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._endpoint);
                if (this._authToken != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                            scheme: "Basic",
                            parameter: this._authToken
                    );
                }
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StreamContent(stream), "file", Path.GetFileName(path));
                request.Content = content;
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Debug.LogError($"XRXP.Trace.FileSender : {response.StatusCode}\n{response}");
                }
                return false;
            }
            finally
            {
                stream.Dispose();
            }
        }

        public void AsyncAdd(string filepath)
        {
            this._queue.Enqueue(filepath);
        }

        public bool Dispose()
        {
            this._running = false;
            this._task.Wait();
            this._task.Dispose();
            return true;
        }

        public void Open(CancellationToken cancellationToken)
        {
            TaskFactory tf = new TaskFactory(cancellationToken, TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning, TaskScheduler.Default);
            this._task = tf.StartNew(this.SendLoop);
        }

        public int RemainingFileCount()
        {
            return this._queue.Count;
        }
    }
}
