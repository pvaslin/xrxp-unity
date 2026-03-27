# Scene Controller Module

The Scene Controller module enables remote management of XR experiments via WebSocket, allowing researchers to control scene transitions from a central server.

## Overview

This module provides:
- **Remote scene switching**: Change scenes from a WebSocket server
- **Real-time notifications**: Broadcast scene state changes
- **Scene validation**: Only allow transitions to valid scenes
- **Event hooks**: Execute custom code on scene changes

## Installation

### Requirements
- Unity 2021.3 LTS or later
- XRXP Core package
- WebSocket server (see configuration)

### Package Installation

```
Add package from git URL:
https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.scenecontroller
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.xrxp.module.scenecontroller": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.scenecontroller"
  }
}
```

## Components

### XRXPSceneController

Main component that manages WebSocket connection and scene transitions.

**Location**: `XRXP/Modules/Setup Scene Controller`

#### Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `ServiceURL` | string | WebSocket server URL | "ws://localhost:8080" |
| `OnChangeScene` | UnityEvent | Events triggered before scene change | Empty |

#### Setup

1. Add scenes to Build Settings (File → Build Settings)
2. Run `XRXP/Modules/Setup Scene Controller`
3. Configure `ServiceURL` in inspector
4. Optionally add events to `OnChangeScene`

#### Usage

```csharp
// Component is added via menu or manually
XRXPSceneController controller = gameObject.AddComponent<XRXPSceneController>();
controller.ServiceURL = "ws://your-server:8080";

// Subscribe to scene change event
controller.OnChangeScene.AddListener(() => {
    Debug.Log("Scene is about to change!");
    // Save state, show transition, etc.
});
```

### Message Structure

WebSocket communication uses JSON messages:

```csharp
public struct Message
{
    public string Protocol;                    // Message type
    public Dictionary<string, string> Properties;  // Key-value data
}
```

#### Protocol: changeScene

Request to change to a different scene:

```json
{
  "protocol": "changeScene",
  "properties": {
    "scene": "ExperimentScene2"
  }
}
```

**Validation**:
- Scene must exist in Build Settings
- Scene name must match exactly (case-sensitive)

## Communication Protocol

### Connection Flow

1. **Initialize**: Component starts and scans Build Settings
2. **Connect**: Opens WebSocket to `ServiceURL/link/{deviceID}`
3. **Announce**: Sends current scene name to server
4. **Listen**: Waits for commands from server
5. **Execute**: Processes scene change requests

### Message Format

#### Client → Server (Scene Change Notification)
```json
{
  "protocol": "changeScene",
  "properties": {
    "scene": "CurrentSceneName"
  }
}
```

#### Server → Client (Scene Change Command)
```json
{
  "protocol": "changeScene",
  "properties": {
    "scene": "TargetSceneName"
  }
}
```

### WebSocket Endpoints

**Connection URL**:
```
ws://your-server:port/link/{deviceUniqueIdentifier}
```

**Example**:
```
ws://localhost:8080/link/ABC123XYZ
```

### Authentication (Optional)

Pass authentication token when connecting:

```csharp
// The controller supports Basic auth via token
// Set token in server implementation
controller.OpenWebsocket(uri, "your-auth-token");
```

## Server Implementation

### Example Node.js Server

```javascript
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 8080 });

const clients = new Map();

wss.on('connection', (ws, req) => {
  // Extract device ID from URL
  const deviceId = req.url.split('/link/')[1];
  clients.set(deviceId, ws);
  
  console.log(`Device connected: ${deviceId}`);
  
  ws.on('message', (data) => {
    const message = JSON.parse(data);
    console.log(`Received: ${message.protocol}`, message.properties);
  });
  
  // Change scene for specific device
  function changeScene(deviceId, sceneName) {
    const client = clients.get(deviceId);
    if (client) {
      client.send(JSON.stringify({
        protocol: 'changeScene',
        properties: { scene: sceneName }
      }));
    }
  }
  
  // Broadcast to all devices
  function broadcastSceneChange(sceneName) {
    clients.forEach((client) => {
      client.send(JSON.stringify({
        protocol: 'changeScene',
        properties: { scene: sceneName }
      }));
    });
  }
});
```

### Example Python Server

```python
import asyncio
import websockets
import json

clients = {}

async def handler(websocket, path):
    # Extract device ID from path
    device_id = path.split('/link/')[1]
    clients[device_id] = websocket
    
    print(f"Device connected: {device_id}")
    
    try:
        async for message in websocket:
            data = json.loads(message)
            print(f"Received: {data}")
    except websockets.exceptions.ConnectionClosed:
        del clients[device_id]

async def change_scene(device_id, scene_name):
    if device_id in clients:
        await clients[device_id].send(json.dumps({
            'protocol': 'changeScene',
            'properties': {'scene': scene_name}
        }))

start_server = websockets.serve(handler, 'localhost', 8080)
asyncio.get_event_loop().run_until_complete(start_server)
asyncio.get_event_loop().run_forever()
```

## Use Cases

### Remote Experiment Control

```csharp
// Server controls experiment flow
// Participant just wears headset

// Server sends:
// 1. "Tutorial" scene
// 2. "Practice" scene  
// 3. "Experiment" scene
// 4. "Survey" scene
```

### Multi-Participant Studies

```csharp
// Synchronize multiple participants
// Server broadcasts same scene to all

void Start() {
    controller.OnChangeScene.AddListener(() => {
        // Fade to black during transition
        StartCoroutine(FadeOut());
    });
}
```

### Automated Testing

```csharp
// Automated test runner changes scenes
// Records performance for each scene

IEnumerator RunTestSequence() {
    yield return ChangeScene("Scene1");
    yield return new WaitForSeconds(10);
    yield return ChangeScene("Scene2");
    yield return new WaitForSeconds(10);
    // ... etc
}
```

## Configuration

### Build Settings Setup

Scenes must be added to Build Settings:

1. File → Build Settings
2. Drag scenes to "Scenes In Build" list
3. Scene names must match exactly

### WebSocket URL

Configure for your environment:

```csharp
// Development
controller.ServiceURL = "ws://localhost:8080";

// Production
controller.ServiceURL = "wss://your-domain.com/ws";

// With path
controller.ServiceURL = "ws://server:8080/xrxp";
```

### Reconnection

The controller attempts to reconnect automatically:
- Maximum 3 connection attempts
- 100ms delay between attempts
- Logs errors if connection fails

## Events

### OnChangeScene

Triggered before scene transition:

```csharp
public UnityEvent OnChangeScene;

// Usage examples:
controller.OnChangeScene.AddListener(() => {
    // Save current state
    SaveManager.Save();
});

controller.OnChangeScene.AddListener(() => {
    // Show loading screen
    UIManager.ShowLoadingScreen();
});

controller.OnChangeScene.AddListener(() => {
    // Stop recording temporarily
    XRXPManager.Recorder.StopSession();
});
```

## Best Practices

1. **Scene Naming**: Use descriptive, consistent names
2. **Validation**: Always test scene names in Build Settings
3. **Error Handling**: Handle connection failures gracefully
4. **User Feedback**: Show visual feedback during transitions
5. **State Management**: Save important state before switching
6. **Network**: Use secure WebSocket (wss://) in production
7. **Cleanup**: Stop unnecessary processes before scene change

## Troubleshooting

### Cannot connect to server
- Verify server is running
- Check firewall settings
- Confirm URL format (ws:// or wss://)
- Check Unity console for errors

### Scene not found
- Verify scene in Build Settings
- Check exact name matching (case-sensitive)
- Ensure scene file extension removed

### Connection drops
- Check network stability
- Verify server keep-alive settings
- Review WebSocket timeout configuration

### Events not triggering
- Ensure component is on XRXPManager
- Check UnityEvent is properly wired
- Verify scene change actually occurs

## Security Considerations

1. **Authentication**: Use tokens for production
2. **Encryption**: Use WSS (WebSocket Secure) in production
3. **Validation**: Validate all scene names server-side
4. **Rate Limiting**: Prevent rapid scene switching
5. **Origin Check**: Validate WebSocket origin

## API Reference

### XRXPSceneController

```csharp
public class XRXPSceneController : MonoBehaviour
{
    public string ServiceURL;
    public UnityEvent OnChangeScene;
    
    private void Awake();
    private async void Start();
    private void ChangedActiveScene(Scene current, Scene next);
    private void OnApplicationQuit();
    private async Task<bool> OpenWebsocket(Uri uri, string authToken = null);
    private bool DisposeWebSocket();
    private async Task<bool> SendData(string data);
    private async Task NotificationHandler();
}
```

### Message

```csharp
public struct Message
{
    public string Protocol;
    public Dictionary<string, string> Properties;
    
    public string ToJson();
    public static string ConvertMessageToJson(Message message);
    public static string EscapeString(string input);
    public static Message ConvertJsonToMessage(string json);
}
```

## Example: Complete Setup

```csharp
// SceneControllerSetup.cs
using UnityEngine;
using XRXP.Modules.SceneController;

public class SceneControllerSetup : MonoBehaviour
{
    [SerializeField] private string serverUrl = "ws://localhost:8080";
    [SerializeField] private GameObject loadingScreen;
    
    void Start()
    {
        // Find or create controller
        var controller = FindObjectOfType<XRXPSceneController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<XRXPSceneController>();
        }
        
        // Configure
        controller.ServiceURL = serverUrl;
        
        // Add events
        controller.OnChangeScene.AddListener(OnSceneChanging);
    }
    
    void OnSceneChanging()
    {
        // Show loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(true);
        
        // Save any important data
        PlayerPrefs.Save();
        
        Debug.Log("Scene is changing...");
    }
}
```

## See Also

- [Modules Overview](./Modules.md)
- [Eye Tracking](./EyeTracking.md)
- [FrameRate Analyser](./FrameRateAnalyser.md)
- [Unity SceneManager Documentation](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html)
