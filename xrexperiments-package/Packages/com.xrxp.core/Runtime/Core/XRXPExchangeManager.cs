using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace XRXP
{
    [Serializable]
    public class ExchangeControlEvent : UnityEvent<string, string> { }

    public class XRXPExchangeManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The ExchangeModality ScriptableObject defining status and control fields")]
        public ExchangeModality Modality;

        [Tooltip("The XRXP Config to use for connection settings. If null, uses XRXPManager's config.")]
        public XRXPConfig Config;

        [Header("Events")]
        [Tooltip("Fired when any control value is received from the dashboard. Args: (key, value)")]
        public ExchangeControlEvent OnControlReceived;

        [Tooltip("Fired when the exchange connection is established")]
        public UnityEvent OnConnected;

        [Tooltip("Fired when the exchange connection is lost")]
        public UnityEvent OnDisconnected;

        private ClientWebSocket _websocket;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _running;
        private string _deviceId;
        private Dictionary<string, string> _statusValues = new Dictionary<string, string>();
        private Dictionary<string, ExchangeControlEvent> _controlEvents = new Dictionary<string, ExchangeControlEvent>();

        public bool IsConnected => _websocket != null && _websocket.State == WebSocketState.Open;

        private void Awake()
        {
            if (Modality == null)
            {
                Debug.LogError("XRXP.Exchange: No ExchangeModality assigned. Please assign one in the inspector.");
                enabled = false;
                return;
            }

            _deviceId = SystemInfo.deviceUniqueIdentifier;
            _cancellationTokenSource = new CancellationTokenSource();

            // Initialize status values with defaults
            foreach (var field in Modality.StatusFields)
            {
                _statusValues[field.Key] = field.DefaultValue ?? "";
            }
        }

        private async void Start()
        {
            await Connect();
        }

        private void OnDestroy()
        {
            _running = false;
            _cancellationTokenSource?.Cancel();
            DisposeWebSocket();
        }

        private void OnApplicationQuit()
        {
            _running = false;
            _cancellationTokenSource?.Cancel();
            DisposeWebSocket();
        }

        public void RegisterControlEvent(string key, ExchangeControlEvent controlEvent)
        {
            _controlEvents[key] = controlEvent;
        }

        public void SetStatus(string key, string value)
        {
            _statusValues[key] = value;
            if (IsConnected)
            {
                SendStatusUpdate(key, value);
            }
        }

        public void SetStatus(string key, float value)
        {
            SetStatus(key, value.ToString("G"));
        }

        public void SetStatus(string key, int value)
        {
            SetStatus(key, value.ToString());
        }

        public void SetStatus(string key, bool value)
        {
            SetStatus(key, value.ToString().ToLower());
        }

        public string GetStatusValue(string key)
        {
            return _statusValues.TryGetValue(key, out string val) ? val : null;
        }

        private async Task Connect()
        {
            var config = Config;
            if (config == null && XRXPManager.IsReady)
            {
                config = FindAnyObjectByType<XRXPManager>()?.config;
            }

            if (config == null)
            {
                Debug.LogError("XRXP.Exchange: No config available. Assign an XRXPConfig or ensure XRXPManager is ready.");
                return;
            }

            string wsUrl = config.WebSocketServer;
            if (string.IsNullOrEmpty(wsUrl))
            {
                Debug.LogError("XRXP.Exchange: WebSocket server URL is not configured.");
                return;
            }

            // Build connection URL with token
            string connectUrl = wsUrl;
            if (!string.IsNullOrEmpty(config.AuthorizationToken))
            {
                string separator = wsUrl.Contains("?") ? "&" : "?";
                connectUrl = $"{wsUrl}{separator}token={config.AuthorizationToken}";
            }

            _websocket = new ClientWebSocket();
            _websocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

            int attempts = 3;
            while (attempts > 0 && _websocket.State != WebSocketState.Open)
            {
                try
                {
                    await _websocket.ConnectAsync(new Uri(connectUrl), _cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"XRXP.Exchange: Connection failed - {ex.Message}");
                    attempts--;
                    if (attempts > 0) await Task.Delay(1000);
                }
            }

            if (_websocket.State != WebSocketState.Open)
            {
                Debug.LogError("XRXP.Exchange: Failed to connect to WebSocket server.");
                return;
            }

            Debug.Log("XRXP.Exchange: Connected to WebSocket server.");
            OnConnected?.Invoke();

            // Send DeviceConnect first
            await SendMessage(BuildDeviceConnectMessage());

            // Register the modality schema
            await SendMessage(BuildModalityMessage());

            // Send initial status values
            await SendMessage(BuildFullStatusMessage());

            // Start listening for incoming messages
            _running = true;
            _ = ListenLoop();
        }

        private async Task ListenLoop()
        {
            byte[] buffer = new byte[4096];

            while (_running && _websocket != null && _websocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _websocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellationTokenSource.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _running = false;
                        OnDisconnected?.Invoke();
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string raw = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessIncomingMessage(raw);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"XRXP.Exchange: Listen error - {ex.Message}");
                    break;
                }
            }

            if (_running)
            {
                _running = false;
                OnDisconnected?.Invoke();
                Debug.LogWarning("XRXP.Exchange: Connection lost. Attempting reconnect...");
                await Task.Delay(2000);
                await Connect();
            }
        }

        private void ProcessIncomingMessage(string json)
        {
            // Simple JSON parsing for exchange control messages
            if (!json.Contains("ExchangeControl")) return;

            // Extract Values object from the message
            var values = ExtractValues(json);
            if (values == null) return;

            foreach (var kvp in values)
            {
                // Fire specific control event if registered
                if (_controlEvents.TryGetValue(kvp.Key, out var specificEvent))
                {
                    UnityMainThreadDispatcher.Enqueue(() => specificEvent?.Invoke(kvp.Key, kvp.Value));
                }

                // Fire generic control event
                UnityMainThreadDispatcher.Enqueue(() => OnControlReceived?.Invoke(kvp.Key, kvp.Value));
            }
        }

        private Dictionary<string, string> ExtractValues(string json)
        {
            var result = new Dictionary<string, string>();

            // Find "Values":{...} in the JSON
            int valuesIdx = json.IndexOf("\"Values\"");
            if (valuesIdx < 0) valuesIdx = json.IndexOf("\"values\"");
            if (valuesIdx < 0) return null;

            int braceStart = json.IndexOf('{', valuesIdx);
            if (braceStart < 0) return null;

            int depth = 0;
            int braceEnd = -1;
            for (int i = braceStart; i < json.Length; i++)
            {
                if (json[i] == '{') depth++;
                if (json[i] == '}') depth--;
                if (depth == 0) { braceEnd = i; break; }
            }

            if (braceEnd < 0) return null;

            string valuesJson = json.Substring(braceStart + 1, braceEnd - braceStart - 1);
            var matches = Regex.Matches(valuesJson, "\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\"");
            foreach (Match match in matches)
            {
                result[match.Groups[1].Value] = match.Groups[2].Value;
            }

            return result.Count > 0 ? result : null;
        }

        private void SendStatusUpdate(string key, string value)
        {
            string json = $"{{\"Protocol\":\"ExchangeStatus\",\"DeviceId\":\"{Escape(_deviceId)}\",\"Values\":{{\"{Escape(key)}\":\"{Escape(value)}\"}}}}";
            _ = SendMessage(json);
        }

        private string BuildDeviceConnectMessage()
        {
            return $"{{\"Protocol\":\"DeviceConnect\",\"DeviceId\":\"{Escape(_deviceId)}\",\"EngineType\":\"Unity\",\"AppVersion\":\"{Escape(Application.version)}\",\"PackageVersion\":\"1.0.0\"}}";
        }

        private string BuildModalityMessage()
        {
            string schema = Modality.ToSchemaJson();
            return $"{{\"Protocol\":\"ExchangeModality\",\"DeviceId\":\"{Escape(_deviceId)}\",\"Schema\":{schema}}}";
        }

        private string BuildFullStatusMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{{\"Protocol\":\"ExchangeStatus\",\"DeviceId\":\"{Escape(_deviceId)}\",\"Values\":{{");

            int i = 0;
            foreach (var kvp in _statusValues)
            {
                sb.Append($"\"{Escape(kvp.Key)}\":\"{Escape(kvp.Value)}\"");
                if (i < _statusValues.Count - 1) sb.Append(",");
                i++;
            }

            sb.Append("}}");
            return sb.ToString();
        }

        private async Task SendMessage(string json)
        {
            if (_websocket == null || _websocket.State != WebSocketState.Open) return;

            try
            {
                var bytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
                await _websocket.SendAsync(bytes, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"XRXP.Exchange: Send error - {ex.Message}");
            }
        }

        private void DisposeWebSocket()
        {
            if (_websocket == null) return;

            try
            {
                if (_websocket.State == WebSocketState.Open)
                {
                    _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait(1000);
                }
            }
            catch { }

            _websocket.Dispose();
            _websocket = null;
        }

        private static string Escape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }

    /// <summary>
    /// Helper to dispatch actions back to the Unity main thread from async callbacks.
    /// Attach this component to the same GameObject as XRXPExchangeManager.
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private static readonly Queue<Action> _queue = new Queue<Action>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (_instance != null) return;
            var go = new GameObject("[XRXP MainThreadDispatcher]");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        public static void Enqueue(Action action)
        {
            if (action == null) return;
            lock (_queue)
            {
                _queue.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (_queue)
            {
                while (_queue.Count > 0)
                {
                    _queue.Dequeue()?.Invoke();
                }
            }
        }
    }
}
