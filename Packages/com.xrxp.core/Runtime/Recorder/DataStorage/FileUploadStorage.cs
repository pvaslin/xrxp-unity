using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using UnityEngine;

namespace XRXP.Recorder.Storage
{
    public class FileUploadStorage : StorageBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly Uri _endpoint;
        private readonly string _authToken;

        public FileUploadStorage(Uri fileServer, string authorizationToken = null)
        {
            _endpoint = fileServer;
            _authToken = authorizationToken;
        }

        protected override void OnOpen() { }

        protected override void ProcessRecord(SerializedRecord record)
        {
            if (string.IsNullOrEmpty(record.FilePath))
            {
                return;
            }
            SendFile(record.FilePath);
        }

        protected override void OnClose() { }

        private void SendFile(string path)
        {
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                FileStream stream;
                try
                {
                    stream = File.OpenRead(path);
                }
                catch (Exception e)
                {
                    Debug.LogError($"XRXP.Recorder [FileUploadStorage]: Cannot open file {path} - {e.Message}");
                    return;
                }

                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, _endpoint))
                    {
                        if (_authToken != null)
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _authToken);
                        }
                        using (var content = new MultipartFormDataContent())
                        {
                            content.Add(new StreamContent(stream), "file", Path.GetFileName(path));
                            request.Content = content;

                            var response = _httpClient.SendAsync(request, CancellationToken)
                                .GetAwaiter().GetResult();

                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                return;
                            }
                            Debug.LogError($"XRXP.Recorder [FileUploadStorage]: Server returned {response.StatusCode}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    stream.Dispose();
                    throw;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"XRXP.Recorder [FileUploadStorage]: Upload attempt {attempt}/{maxAttempts} failed: {e.Message}");
                }
                finally
                {
                    stream.Dispose();
                }

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(5000);
                }
            }

            Debug.LogError($"XRXP.Recorder [FileUploadStorage]: Failed to upload {path} after {maxAttempts} attempts.");
        }
    }
}
