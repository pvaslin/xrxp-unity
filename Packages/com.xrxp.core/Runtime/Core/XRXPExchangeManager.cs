using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace XRXP
{
    /// <summary>
    /// Serialized binding between a control field key and a UnityEvent.
    /// Auto-synced from the ExchangeModality by the Custom Editor.
    /// </summary>
    [Serializable]
    public class ExchangeEventBinding
    {
        public string Key;
        public string Label;
        public UnityEvent<string> OnReceived;
    }

    /// <summary>
    /// Tracks a method discovered via [ExchangeControl] attribute at runtime.
    /// </summary>
    public class AttributeControlHandler
    {
        public string Key;
        public MonoBehaviour Target;
        public MethodInfo Method;
        public string TargetDescription;

        public void Invoke(string value)
        {
            Method.Invoke(Target, new object[] { value });
        }
    }

    public class XRXPExchangeManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The ExchangeModality ScriptableObject defining status and control fields")]
        public ExchangeModality Modality;

        [Tooltip("The XRXP Config to use for connection settings. If null, uses XRXPManager's config.")]
        public XRXPConfig Config;

        [Header("Control Event Bindings")]
        [Tooltip("Auto-synced from Modality. Wire UnityEvents per control field in the Inspector.")]
        public List<ExchangeEventBinding> ControlBindings = new List<ExchangeEventBinding>();

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

        // Attribute-discovered handlers (populated at startup)
        private List<AttributeControlHandler> _attributeHandlers = new List<AttributeControlHandler>();

        // Fast lookup for serialized bindings by key
        private Dictionary<string, ExchangeEventBinding> _bindingLookup = new Dictionary<string, ExchangeEventBinding>();

        public bool IsConnected => _websocket != null && _websocket.State == WebSocketState.Open;

        /// <summary>
        /// Read-only access to discovered attribute handlers (used by Custom Editor).
        /// </summary>
        public IReadOnlyList<AttributeControlHandler> AttributeHandlers => _attributeHandlers;

        /// <summary>
        /// Read-only access to current status values (used by Custom Editor for live preview).
        /// </summary>
        public IReadOnlyDictionary<string, string> StatusValues => _statusValues;

        private void Awake()
        {
            _deviceId = SystemInfo.deviceUniqueIdentifier;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async void Start()
        {
            if (Modality == null)
            {
                Debug.LogError("XRXP.Exchange: No ExchangeModality assigned. Please assign one in the inspector.");
                enabled = false;
                return;
            }

            // Initialize status values with defaults
            foreach (var field in Modality.StatusFields)
            {
                _statusValues[field.Key] = field.DefaultValue ?? "";
            }

            // Build binding lookup
            foreach (var binding in ControlBindings)
            {
                if (!string.IsNullOrEmpty(binding.Key))
                {
                    _bindingLookup[binding.Key] = binding;
                }
            }

            // Discover [ExchangeControl] attribute handlers
            DiscoverAttributeHandlers();

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

        // ── Status API ──────────────────────────────────────────────────

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

        // ── Attribute Discovery ─────────────────────────────────────────

        public void DiscoverAttributeHandlers()
        {
            _attributeHandlers.Clear();

            // Scan all MonoBehaviours in the scene
            var components = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var component in components)
            {
                if (component == null || component == this) continue;

                var type = component.GetType();
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(ExchangeControlAttribute), true);
                    foreach (ExchangeControlAttribute attr in attributes)
                    {
                        // Validate method signature: must accept a single string parameter
                        var parameters = method.GetParameters();
                        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string))
                        {
                            Debug.LogWarning(
                                $"XRXP.Exchange: Method {type.Name}.{method.Name} has [ExchangeControl(\"{attr.Key}\")] " +
                                $"but wrong signature. Expected (string). Skipping.");
                            continue;
                        }

                        _attributeHandlers.Add(new AttributeControlHandler
                        {
                            Key = attr.Key,
                            Target = component,
                            Method = method,
                            TargetDescription = $"{type.Name}.{method.Name}"
                        });

                        Debug.Log($"XRXP.Exchange: Discovered [{attr.Key}] -> {type.Name}.{method.Name}()");
                    }
                }
            }
        }

        // ── Connection ──────────────────────────────────────────────────

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

            string connectUrl = wsUrl;
            if (!string.IsNullOrEmpty(config.AuthorizationToken))
            {
                string separator = wsUrl.Contains("?") ? "&" : "?";
                connectUrl = $"{wsUrl}{separator}token={config.AuthorizationToken}";
            }

            _websocket = new ClientWebSocket();
            _websocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

            int attempts = 3;
            while (attempts > 0 && _websocket != null && _websocket.State != WebSocketState.Open)
            {
                try
                {
                    await _websocket.ConnectAsync(new Uri(connectUrl), _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"XRXP.Exchange: Connection failed - {ex.Message}");
                    attempts--;
                    if (attempts > 0) await Task.Delay(1000);
                }
            }

            if (_websocket == null || _websocket.State != WebSocketState.Open)
            {
                Debug.LogError("XRXP.Exchange: Failed to connect to WebSocket server.");
                return;
            }

            Debug.Log("XRXP.Exchange: Connected to WebSocket server.");
            OnConnected?.Invoke();

            await SendMessage(BuildDeviceConnectMessage());
            await SendMessage(BuildModalityMessage());
            await SendMessage(BuildFullStatusMessage());

            _running = true;
            _ = ListenLoop().ContinueWith(
                t => Debug.LogError($"XRXP.Exchange: ListenLoop faulted - {t.Exception?.GetBaseException()?.Message}"),
                System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
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

        // ── Message Processing ──────────────────────────────────────────

        private void ProcessIncomingMessage(string json)
        {
            if (!json.Contains("ExchangeControl")) return;

            var values = ExtractValues(json);
            if (values == null) return;

            foreach (var kvp in values)
            {
                string key = kvp.Key;
                string value = kvp.Value;

                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    // 1) Fire serialized Inspector binding
                    if (_bindingLookup.TryGetValue(key, out var binding))
                    {
                        binding.OnReceived?.Invoke(value);
                    }

                    // 2) Fire attribute-discovered handlers
                    foreach (var handler in _attributeHandlers)
                    {
                        if (handler.Key == key && handler.Target != null)
                        {
                            try
                            {
                                handler.Invoke(value);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"XRXP.Exchange: Error in [{key}] -> {handler.TargetDescription}: {ex.Message}");
                            }
                        }
                    }

                    // 3) Fire generic event
                    OnControlReceived?.Invoke(key, value);
                });
            }
        }

        private Dictionary<string, string> ExtractValues(string json)
        {
            var result = new Dictionary<string, string>();

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

        // ── Message Building ────────────────────────────────────────────

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

    [Serializable]
    public class ExchangeControlEvent : UnityEvent<string, string> { }

    /// <summary>
    /// Dispatches actions back to the Unity main thread from async callbacks.
    /// Auto-created at runtime.
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
