using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace XRXP.Modules.SceneController
{
    /// <summary>
    /// WebSocket-based scene controller for remote experiment management.
    /// Allows remote control of scene changes via WebSocket connection.
    /// </summary>
    public class XRXPSceneController : MonoBehaviour
    {
        private static XRXPSceneController _singleton = null;
        private const int MaxAttempt = 3;
        private ClientWebSocket _websocket;
        private CancellationTokenSource _cancellationTokenSource;
        
        [Header("WebSocket Service URL")]
        [Tooltip("URL of the XRXP scene controller service")]
        public string ServiceURL;
        
        [Header("Events")]
        public UnityEvent OnChangeScene;
        
        private List<string> _sceneNames;

        private void Awake()
        {
            // Verify if the Experiment component is not already started
            if (_singleton == null)
            {
                _singleton = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (!XRXPManager.IsReady)
            {
                Debug.LogWarning("XRXP: Scene Controller is already set in the scene");
                Destroy(this.gameObject);
                return;
            }
        }

        async void Start()
        {
            this._sceneNames = new List<string>();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                int lastSlash = scenePath.LastIndexOf("/");
                this._sceneNames.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
            }

            // Setup scene changes
            SceneManager.activeSceneChanged += this.ChangedActiveScene;

            // Open websocket
            this._cancellationTokenSource = new CancellationTokenSource();
            if (!await this.OpenWebsocket(new Uri($"{this.ServiceURL}/link/{SystemInfo.deviceUniqueIdentifier}")))
            {
                throw new XRXPException("The XRXP server is not reachable, please check if the URL is correct.");
            }
            
            Message message = new Message
            {
                Protocol = "changeScene",
                Properties = new Dictionary<string, string>() { { "scene", SceneManager.GetActiveScene().name } }
            };
            
            await this.SendData(message.ToJson());
            this.NotificationHandler();
        }

        void ChangedActiveScene(Scene current, Scene next)
        {
            if (this._websocket == null || this._websocket.State != WebSocketState.Open)
            {
                return;
            }
            
            Message message = new Message
            {
                Protocol = "changeScene",
                Properties = new Dictionary<string, string>() { { "scene", next.name } }
            };
            
            this.SendData(message.ToJson());
        }

        void OnApplicationQuit()
        {
            this.DisposeWebSocket();
        }

        void OnDestroy()
        {
            this.DisposeWebSocket();
        }

        private async Task<bool> OpenWebsocket(Uri uri, string authToken = null)
        {
            this._websocket = new ClientWebSocket();
            if (authToken != null && authToken.Length > 0)
            {
                this._websocket.Options.SetRequestHeader("Authorization", "Basic " + authToken);
            }
            this._websocket.Options.KeepAliveInterval = new TimeSpan(0, 0, 5);
            
            int attempts = MaxAttempt;
            while (attempts > 0 && (this._websocket.State != WebSocketState.Open))
            {
                try
                {
                    await this._websocket.ConnectAsync(uri, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"XRXP.SceneController: {exception.Message}");
                    attempts -= 1;
                    await Task.Delay(100);
                }
            }
            
            return this._websocket.State == WebSocketState.Open;
        }

        private bool DisposeWebSocket()
        {
            if (this._websocket == null)
                return true;
                
            try
            {
                this._websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", this._cancellationTokenSource.Token);
            }
            catch (Exception exception)
            {
                Debug.LogError($"XRXP.SceneController: {exception.Message}");
            }
            
            return this._websocket.State == WebSocketState.Closed;
        }

        private async Task<bool> SendData(string data)
        {
            if (this._websocket == null)
            {
                throw new XRXPException("The websocket is not open, please use OpenWebsocket() before sending data");
            }
            
            if (this._websocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
                try
                {
                    await this._websocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogError($"XRXP.SceneController: {exception.Message}");
                }
            }
            
            return false;
        }

        private async Task NotificationHandler()
        {
            int maxMessageSize = 2048;
            
            while (this._websocket != null && this._websocket.State == WebSocketState.Open)
            {
                try
                {
                    ArraySegment<byte> bytesBuffer = new ArraySegment<byte>(new byte[maxMessageSize]);
                    WebSocketReceiveResult receiveResult = await this._websocket.ReceiveAsync(bytesBuffer, CancellationToken.None);
                    
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await this._websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        await this._websocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Cannot accept binary frame", CancellationToken.None);
                    }
                    else
                    {
                        ArraySegment<byte> buffer = bytesBuffer.Slice(0, receiveResult.Count);
                        string rawMessage = Encoding.UTF8.GetString(buffer);
                        Debug.Log(rawMessage);
                        
                        try
                        {
                            Message message = Message.ConvertJsonToMessage(rawMessage);
                            if (message.Protocol == "changeScene")
                            {
                                if (message.Properties.ContainsKey("scene") && this._sceneNames.Contains(message.Properties["scene"]))
                                {
                                    Debug.Log($"Changing scene to {message.Properties["scene"]}");
                                    this.OnChangeScene?.Invoke();
                                    SceneManager.LoadScene(message.Properties["scene"]);
                                }
                                else if (message.Properties.ContainsKey("scene"))
                                {
                                    Debug.Log($"No scene by this name [{message.Properties["scene"]}]");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"XRXP.SceneController: Error processing message - {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"XRXP.SceneController: WebSocket error - {ex.Message}");
                    break;
                }
                
                await Task.Delay(10);
            }
        }
    }
}
