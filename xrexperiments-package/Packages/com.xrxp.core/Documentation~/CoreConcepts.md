# Core Concepts

Understanding the core components of XR Experiments (XRXP) framework.

## XRXPManager

The central hub that manages the XRXP system lifecycle and provides access to the recorder.

### Overview

`XRXPManager` is a singleton MonoBehaviour that:
- Ensures only one XRXP instance exists per scene
- Persists across scene loads (`DontDestroyOnLoad`)
- Initializes the recording system
- Provides access to `XRXPRecorder`

### Setup

**Automatic Setup (Recommended)**:
```
Menu: XRXP > Setup the scene
```

**Manual Setup**:
1. Create empty GameObject named "XRXPManager"
2. Add `XRXPManager` component
3. Create and assign `XRXPConfig` asset

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Recorder` | `XRXPRecorder` | Access to recording API (static) |
| `IsReady` | `bool` | Indicates if XRXP is initialized (static) |
| `config` | `XRXPConfig` | Configuration asset reference |

### Usage

```csharp
// Check if XRXP is ready
if (XRXPManager.IsReady)
{
    // Access the recorder
    XRXPManager.Recorder.StartSession("Participant 001");
}
```

### Important Notes

- **Singleton Pattern**: Only one XRXPManager can exist. Additional instances are automatically destroyed.
- **Initialization**: Don't call `XRXPManager.Recorder` in `Awake()` methods. Use `Start()` or later.
- **Scene Persistence**: XRXPManager survives scene changes, maintaining session continuity.

### Error Handling

```csharp
// Wrong - will throw exception
text
void Awake()
{
    XRXPManager.Recorder.AddLogEvent("System", "start", "game"); // ERROR!
}

// Correct
text
void Start()
{
    if (XRXPManager.IsReady)
    {
        XRXPManager.Recorder.AddLogEvent("System", "start", "game");
    }
}
```

---

## XRXPConfig

Configuration asset that defines experiment settings and server connections.

### Overview

`XRXPConfig` is a ScriptableObject that stores:
- Experiment identification
- Server connection settings
- Storage preferences
- Authorization tokens

### Creating Config

**Automatic**: Created by "Setup the scene" menu item in `Assets/XRXPConfig/`

**Manual**:
1. Right-click in Project window
2. Select **XRXP > XRXPConfig**
3. Configure settings in Inspector

### Configuration Properties

#### Experiment Settings

| Property | Type | Description |
|----------|------|-------------|
| `ExperimentId` | string | Unique identifier for your experiment |

#### Server Settings

| Property | Type | Description |
|----------|------|-------------|
| `Online` | bool | Enable WebSocket server connection |
| `WebSocketServer` | string | WebSocket server URL (e.g., `ws://localhost:8080`) |
| `FileServer` | string | File upload server URL |
| `AuthorizationToken` | string | API authentication token |

#### Storage Settings

| Property | Type | Description |
|----------|------|-------------|
| `LocalStoragePath` | string | Path for local data storage |
| `EnableBackup` | bool | Enable backup storage |

### Example Configuration

```csharp
// Reference to config (if you need to change at runtime)
XRXPConfig config = FindObjectOfType<XRXPManager>().config;

// Modify experiment ID
config.ExperimentId = "Experiment_2024_Study_A";
```

### Security

- Store sensitive tokens (API keys) in the config asset
- The config file is stored in your project, not included in version control by default
- Consider using environment variables for production tokens

---

## XRXPRecorder

The main API for recording experiment data, managing sessions, and storing traces.

### Overview

`XRXPRecorder` provides methods to:
- Start and stop recording sessions
- Log events, questions, and statistics
- Track object positions and media
- Manage data storage and transmission

### Session Management

#### StartSession

Begins a new recording session.

```csharp
string StartSession(string comments = "", string userId = "")
```

**Parameters**:
- `comments`: Optional description or notes
- `userId`: Optional custom user identifier (auto-generated UUID if empty)

**Returns**: Session ID string

**Example**:
```csharp
string sessionId = XRXPManager.Recorder.StartSession(
    comments: "VR Comfort Study - Condition A",
    userId: "Participant_001"
);
Debug.Log($"Started session: {sessionId}");
```

#### StopSession

Ends the current recording session.

```csharp
void StopSession()
```

**Example**:
```csharp
// End the session
XRXPManager.Recorder.StopSession();

// Data is automatically flushed and uploaded
```

### Recording Methods

#### AddLogEvent

Record a generic event with actor-action-value structure.

```csharp
void AddLogEvent(string actor, string action, string value)
```

**Example**:
```csharp
// User interactions
XRXPManager.Recorder.AddLogEvent("User", "clicked", "Button_A");
XRXPManager.Recorder.AddLogEvent("User", "grabbed", "Cube_1");

// System events
XRXPManager.Recorder.AddLogEvent("System", "loaded", "Level_2");
XRXPManager.Recorder.AddLogEvent("System", "error", "Connection timeout");
```

#### AddQuestion

Record questionnaire responses.

```csharp
void AddQuestion(string name, string value)
```

**Example**:
```csharp
// Record questionnaire answers
XRXPManager.Recorder.AddQuestion("Comfort_Rating", "7");
XRXPManager.Recorder.AddQuestion("Simulator_Sickness", "Low");
XRXPManager.Recorder.AddQuestion("Presence_Score", "High");
```

#### AddStatistic

Record numerical statistics.

```csharp
void AddStatistic(string name, float value)
```

**Example**:
```csharp
// Performance metrics
XRXPManager.Recorder.AddStatistic("FPS_Average", 88.5f);
XRXPManager.Recorder.AddStatistic("Completion_Time", 245.3f);

// Custom metrics
XRXPManager.Recorder.AddStatistic("Object_Distance_Traveled", 15.7f);
```

#### AddMediaEvent

Record media playback events.

```csharp
void AddMediaEvent(string type, string name, string action, long timestamp)
```

**Example**:
```csharp
// Video events
XRXPManager.Recorder.AddMediaEvent(
    type: "Video", 
    name: "Tutorial_Video", 
    action: "started", 
    timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
```

### Object Tracking

#### AddInternalEvent

Record internal system events (used by trackers).

```csharp
void AddInternalEvent(SystemType type, string category, string name, object data)
```

**Example**:
```csharp
// Track position (usually handled automatically by XRXPObjectTracker)
WorldPosition wp = new WorldPosition(transform.position, transform.rotation);
XRXPManager.Recorder.AddInternalEvent(
    SystemType.WorldPosition, 
    "Body", 
    "Head", 
    wp
);
```

### Data Management

#### RemainingTraceCount

Check how many traces are pending upload.

```csharp
long RemainingTraceCount()
```

**Example**:
```csharp
// Check if all data is uploaded before quitting
if (XRXPManager.Recorder.RemainingTraceCount() > 0)
{
    Debug.Log($"Waiting for {count} traces to upload...");
}
```

#### EndTracing

Safely end tracing and wait for all data to be uploaded.

```csharp
async void EndTracing()
```

**Example**:
```csharp
// Call before application quit
void OnApplicationQuit()
{
    XRXPManager.Recorder.EndTracing();
    // Waits for all pending traces to be sent
}
```

### Status Methods

#### isRecording

Check if a session is currently active.

```csharp
bool isRecording()
```

**Example**:
```csharp
void Update()
{
    if (XRXPManager.IsReady && XRXPManager.Recorder.isRecording())
    {
        // Only record when session is active
        RecordCustomData();
    }
}
```

---

## Data Flow

Understanding how data flows through XRXP:

```
1. Your Code
   ↓
2. XRXPManager.Recorder.Method()
   ↓
3. DataManager (queue and buffer)
   ↓
4. Storage Backends
   ├── LocalStorage (file system)
   ├── RemoteStorage (WebSocket)
   └── BackupStorage (redundancy)
   ↓
5. FileSender (upload to server)
```

### Storage Priority

1. **Primary**: Local file storage (always enabled)
2. **Secondary**: WebSocket streaming (if Online enabled)
3. **Backup**: Backup storage (if enabled)

### Data Format

All data is recorded in structured format:

```json
{
  "experimentId": "Experiment_001",
  "sessionId": "uuid-session",
  "timestamp": "2024-01-15T10:30:00Z",
  "events": [...],
  "internalEvents": [...],
  "questions": [...],
  "statistics": [...],
  "mediaEvents": [...]
}
```

---

## Best Practices

### Session Management

1. **Start Early**: Begin session at experiment start
2. **End Cleanly**: Always call `StopSession()` before scene changes or quit
3. **Check Status**: Verify `isRecording()` before adding data
4. **User IDs**: Use consistent, meaningful user identifiers

### Data Recording

1. **Be Consistent**: Use consistent naming for actors, actions, and values
2. **Timestamps**: Don't worry about timestamps - XRXP handles them automatically
3. **Volume**: Don't record every frame unless necessary (use XRXPObjectTracker.TraceFrequency)
4. **Categories**: Group related events under logical categories

### Error Handling

```csharp
try
{
    if (XRXPManager.IsReady)
    {
        XRXPManager.Recorder.AddLogEvent("User", "action", "value");
    }
}
catch (XRXPException ex)
{
    Debug.LogError($"XRXP Error: {ex.Message}");
}
```

---

## See Also

- [Getting Started](GettingStarted.md) - Quick start guide
- [Tracking](Tracking.md) - Object tracking and events
- [Modules](Modules.md) - Optional modules
- [API Reference](API_REFERENCE.md) - Complete API documentation
