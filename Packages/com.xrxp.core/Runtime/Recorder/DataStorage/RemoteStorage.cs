using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace XRXP.Recorder.Storage
{
    public class RemoteStorage : StorageBase
    {
        private const int MaxConnectAttempts = 3;
        private const int MaxSendAttempts = 3;

        private ClientWebSocket _websocket;
        private readonly Uri _uri;
        private readonly string _authToken;

        public RemoteStorage(Uri uri, string authorizationToken = null)
        {
            _uri = uri;
            _authToken = authorizationToken;
        }

        protected override void OnOpen()
        {
            ConnectWebSocket();
        }

        protected override void ProcessRecord(SerializedRecord record)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(record.Json);
            var segment = new ArraySegment<byte>(bytes);

            for (int attempt = 1; attempt <= MaxSendAttempts; attempt++)
            {
                try
                {
                    if (_websocket == null || _websocket.State != WebSocketState.Open)
                    {
                        ConnectWebSocket();
                    }
                    _websocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken)
                        .GetAwaiter().GetResult();
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"XRXP.Recorder [RemoteStorage]: Send attempt {attempt}/{MaxSendAttempts} failed: {e.Message}");
                    if (attempt < MaxSendAttempts)
                    {
                        DisposeWebSocket();
                        Thread.Sleep(1000);
                    }
                }
            }

            Debug.LogError($"XRXP.Recorder [RemoteStorage]: Failed to send record {record.Id} after {MaxSendAttempts} attempts.");
        }

        protected override void OnClose()
        {
            CloseWebSocketGracefully();
            DisposeWebSocket();
        }

        private void ConnectWebSocket()
        {
            DisposeWebSocket();

            _websocket = new ClientWebSocket();
            if (_authToken != null)
            {
                _websocket.Options.SetRequestHeader("Authorization", "Basic " + _authToken);
            }
            _websocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(1);

            for (int attempt = 1; attempt <= MaxConnectAttempts; attempt++)
            {
                try
                {
                    _websocket.ConnectAsync(_uri, CancellationToken)
                        .GetAwaiter().GetResult();
                    if (_websocket.State == WebSocketState.Open)
                    {
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Debug.LogError($"XRXP.Recorder [RemoteStorage]: Connect attempt {attempt}/{MaxConnectAttempts} failed: {e.Message}");
                    if (attempt < MaxConnectAttempts)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            Debug.LogWarning($"XRXP.Recorder [RemoteStorage]: Could not connect to {_uri}");
        }

        private void CloseWebSocketGracefully()
        {
            if (_websocket != null && _websocket.State == WebSocketState.Open)
            {
                try
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                    {
                        _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cts.Token)
                            .GetAwaiter().GetResult();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"XRXP.Recorder [RemoteStorage]: Graceful close failed: {e.Message}");
                }
            }
        }

        private void DisposeWebSocket()
        {
            if (_websocket != null)
            {
                try
                {
                    _websocket.Dispose();
                }
                catch (Exception) { }
                _websocket = null;
            }
        }
    }
}
