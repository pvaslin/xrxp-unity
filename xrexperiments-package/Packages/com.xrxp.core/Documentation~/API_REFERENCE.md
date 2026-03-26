# XR Experiments (XRXP) - Complete User Guide

This document provides comprehensive documentation for using the XR Experiments (XRXP) Unity package for XR experimentation and data recording.

## Table of Contents

1. [Setup and Installation](#setup-and-installation)
2. [Configuration](#configuration)
3. [XRXPRecorder API](#xrxprecorder-api)
4. [Object Tracking](#object-tracking)
5. [Storage Options](#storage-options)
6. [Data Types and Events](#data-types-and-events)
7. [Code Examples](#code-examples)

---

## Setup and Installation

### Package Installation

Add to your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.xrxp.core": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.core",
    "com.xrxp.module.eyetracking": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.eyetracking"
  }
}
```

### Scene Setup

**Method 1: Using the Menu (Recommended)**

1. In Unity Editor, go to menu: **XRXP > Setup the scene**
2. This automatically creates:
   - An `XRXP` GameObject with `XRXPManager` component in your scene
   - A `XRXPConfig` ScriptableObject in `Assets/XRXPConfig/`

**Method 2: Manual Setup**

1. Create empty GameObject named "XRXP"
2. Add `XRXPManager` component
3. Create `XRXPConfig` asset via right-click menu: **XRXP > XRXPConfig**
4. Assign the config to the XRXPManager

---

## Configuration

### XRXPConfig Settings

The `XRXPConfig` ScriptableObject controls all recording settings:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **ExperimentID** | string | "Experimentation" | Unique identifier for your experiment |
| **OnlineMode** | bool | true | Enable/disable WebSocket server connection |
| **BackUpStorageMode** | bool | true | Enable local backup storage (always enabled) |
| **LocalStorageMode** | bool | false | Enable local JSON file storage (Not yet implemented) |
| **WebSocketServer** | string | null | WebSocket server URL (e.g., `ws://localhost:8080`) |
| **FileServer** | string | null | HTTP server URL for file uploads |
| **AuthorizationToken** | string | null | API token for authentication |
| **RESTServer** | string | "https://..." | REST API endpoint (reserved for future use) |

### Configuration Example

```csharp
// Access and modify config at runtime
XRXPManager manager = FindObjectOfType<XRXPManager>();
manager.config.ExperimentID = "MyExperiment_v1";
manager.config.OnlineMode = true;
manager.config.WebSocketServer = "ws://myserver.com:8080";
```

---

## XRXPRecorder API

Access the recorder via: `XRXPManager.Recorder`

### Session Management

#### StartSession
```csharp
public string StartSession(
    string comments = "", 
    string userId = "", 
    Dictionary<string, string> environmentProperties = null, 
    string environmentId = null
)
```

Starts a new recording session.

**Parameters:**
- `comments` - Optional description of the session/user role
- `userId` - Custom user ID (auto-generated UUID if empty)
- `environmentProperties` - Key-value pairs describing the environment (e.g., room, lighting)
- `environmentId` - Reference to an existing environment in database

**Returns:** Session ID string

**Example:**
```csharp
string sessionId = XRXPManager.Recorder.StartSession(
    "Participant 001 - VR Training",
    "user-123",
    new Dictionary<string, string> {
        { "room", "lab-1" },
        { "lighting", "dim" }
    }
);
```

#### StopSession
```csharp
public void StopSession()
```

Stops the current session and records the end time.

**Example:**
```csharp
XRXPManager.Recorder.StopSession();
```

#### JoinSession
```csharp
public void JoinSession(
    string sessionId, 
    string comments, 
    string userId = "", 
    Dictionary<string, string> environmentProperties = null, 
    string environmentId = ""
)
```

Join an existing multi-user session.

**Parameters:**
- `sessionId` - ID of the session to join
- `comments` - Role description for the joining user
- `userId` - Custom user ID (auto-generated if empty)
- `environmentProperties` - Environment specifications
- `environmentId` - Existing environment reference

**Example:**
```csharp
XRXPManager.Recorder.JoinSession(
    "session-abc-123",
    "Observer role",
    "observer-001"
);
```

### Session Information

#### GetSessionId
```csharp
public string GetSessionId()
```

Returns the current session ID.

#### GetEnvironnementId
```csharp
public string GetEnvironnementId()
```

Returns the current environment ID (note: typo preserved from API).

#### GetUserId
```csharp
public string GetUserId()
```

Returns the current user ID.

#### isRecording
```csharp
public bool isRecording()
```

Returns `true` if a session is currently active.

**Example:**
```csharp
if (XRXPManager.Recorder.isRecording()) {
    Debug.Log("Currently recording...");
}
```

### Event Recording

#### AddLogEvent
```csharp
public void AddLogEvent(
    string actor, 
    string verb, 
    string @object, 
    Dictionary<string, string> properties = null
)
```

Records a semantic event (Actor-Verb-Object pattern).

**Parameters:**
- `actor` - Who performed the action (e.g., "User", "System", "NPC")
- `verb` - The action performed (e.g., "clicked", "grabbed", "looked_at")
- `@object` - The target of the action (e.g., "Button_A", "Cube", "Menu")
- `properties` - Optional key-value properties

**Example:**
```csharp
// Simple event
XRXPManager.Recorder.AddLogEvent("User", "clicked", "StartButton");

// Event with properties
XRXPManager.Recorder.AddLogEvent(
    "User", 
    "grabbed", 
    "RedCube",
    new Dictionary<string, string> {
        { "hand", "right" },
        { "pressure", "0.8" },
        { "position", "(1.5, 0.2, 3.0)" }
    }
);
```

#### AddInternalEvent
```csharp
public void AddInternalEvent(
    SystemType systemType, 
    string systemName, 
    string property, 
    Jsonable value
)
```

Records internal system data (position, rotation, sensor values).

**Parameters:**
- `systemType` - Type of data (`SystemType.WorldPosition` or `SystemType.QuantitativeValue`)
- `systemName` - Name of the tracking system (e.g., "HeadTracker", "LeftHand", "Sensors")
- `property` - Specific property name (e.g., "Head", "Controller", "Temperature")
- `value` - Value object (must implement `Jsonable` interface)

**SystemType Values:**
- `SystemType.WorldPosition` - 3D position and rotation
- `SystemType.QuantitativeValue` - Numeric sensor values

**Example:**
```csharp
// Track head position
WorldPosition headPos = new WorldPosition(
    Camera.main.transform.position, 
    Camera.main.transform.rotation
);
XRXPManager.Recorder.AddInternalEvent(
    SystemType.WorldPosition, 
    "HMD", 
    "Head", 
    headPos
);

// Track sensor value
QuantitativeValue heartRate = new QuantitativeValue(72, "bpm");
XRXPManager.Recorder.AddInternalEvent(
    SystemType.QuantitativeValue, 
    "Biometrics", 
    "HeartRate", 
    heartRate
);
```

#### AddQuestion
```csharp
public void AddQuestion(
    string label, 
    string answer, 
    Dictionary<string, string> properties = null
)
```

Records a question-response pair.

**Parameters:**
- `label` - Question text or identifier
- `answer` - User's answer
- `properties` - Additional context

**Example:**
```csharp
XRXPManager.Recorder.AddQuestion(
    "Comfort Level",
    "High",
    new Dictionary<string, string> {
        { "scale", "1-5" },
        { "question_type", "likert" }
    }
);
```

### Media Recording

#### AddMediaEvent (File Path)
```csharp
public void AddMediaEvent(
    string filePath, 
    string mimeType, 
    string name, 
    int duration = 0
)
```

Records a media file reference.

**Parameters:**
- `filePath` - Absolute path to the file
- `mimeType` - MIME type (e.g., "image/png", "video/mp4")
- `name` - Descriptive name
- `duration` - Duration in milliseconds (for audio/video)

**Example:**
```csharp
XRXPManager.Recorder.AddMediaEvent(
    "/storage/screenshot_001.png",
    "image/png",
    "User View - Start",
    0
);
```

#### AddMediaEvent (Bytes)
```csharp
public void AddMediaEvent(
    byte[] bytes, 
    string mimeType, 
    string name, 
    int duration = 0
)
```

Records media from byte array (auto-uploads to file server).

**Example:**
```csharp
byte[] imageBytes = File.ReadAllBytes("screenshot.png");
XRXPManager.Recorder.AddMediaEvent(
    imageBytes,
    "image/png",
    "User View - End",
    0
);
```

### Metadata

#### AddEnvironmentProperties
```csharp
public void AddEnvironmentProperties(Dictionary<string, string> properties = null)
```

Adds properties to the current environment.

**Example:**
```csharp
XRXPManager.Recorder.AddEnvironmentProperties(
    new Dictionary<string, string> {
        { "temperature", "22C" },
        { "noise_level", "low" },
        { "time_of_day", "afternoon" }
    }
);
```

#### AddUserProperties
```csharp
public void AddUserProperties(Dictionary<string, string> properties = null)
```

Adds properties to the current user.

**Example:**
```csharp
XRXPManager.Recorder.AddUserProperties(
    new Dictionary<string, string> {
        { "age", "25" },
        { "experience", "beginner" },
        { "gender", "female" }
    }
);
```

### Utility Methods

#### TransfersState
```csharp
public int TransfersState()
```

Returns the number of remaining traces to be sent/stored.

**Example:**
```csharp
int pending = XRXPManager.Recorder.TransfersState();
Debug.Log($"Pending traces: {pending}");
```

#### EndTracing
```csharp
public async void EndTracing()
```

Gracefully stops all storage services after sending remaining traces.

**Example:**
```csharp
// Call when experiment ends
XRXPManager.Recorder.EndTracing();
```

---

## Object Tracking

### XRXPObjectTracker Component

Automatically tracks GameObject position and rotation.

#### Setup

1. Select any GameObject in your scene
2. Add Component: **XRXP > XRXPObjectTracker**
3. Configure in Inspector:

| Property | Description |
|----------|-------------|
| **Tracing Enabled** | Toggle recording on/off |
| **Category** | Type of object (e.g., "Body") |
| **Object Name** | Name of object (e.g., "Hand", "Head", "Prop") |
| **Trace Frequency** | How often to record (0 = every frame) |

#### Usage Examples

**Track Player Hands:**
```
GameObject: LeftHand
Category: "Hand"
Object Name: "LeftHand"
Trace Frequency: 0 (every frame for smooth tracking)
```

**Track Static Objects:**
```
GameObject: ExperimentTable
Category: "Prop"
Object Name: "Table"
Trace Frequency: 30 (every 30 frames to save data)
```

**Track Eye Gaze:**
```
GameObject: EyeTracker
Category: "Eye"
Object Name: "EyeGaze"
Trace Frequency: 5
```

#### Custom Tracking Script

Extend `XRXPObjectTracker` for custom behavior:

```csharp
using XRXP.Recorder;
using XRXP.Recorder.Models;

public class CustomTracker : XRXPObjectTracker
{
    public override string GetObjectName()
    {
        // Custom naming logic
        return $"{Category}_{ObjectName}_{Time.frameCount}";
    }
    
    internal override void Record()
    {
        // Custom recording logic
        if (XRXPManager.IsReady && this.TracingEnabled && XRXPManager.Recorder.isRecording())
        {
            // Track custom data
            WorldPosition wp = new WorldPosition(transform.position, transform.rotation);
            XRXPManager.Recorder.AddInternalEvent(
                SystemType.WorldPosition, 
                this.Category, 
                this.GetObjectName(), 
                wp
            );
            
            // Add additional metrics
            QuantitativeValue velocity = new QuantitativeValue(
                GetComponent<Rigidbody>().velocity.magnitude, 
                "m/s"
            );
            XRXPManager.Recorder.AddInternalEvent(
                SystemType.QuantitativeValue,
                this.Category,
                $"{this.GetObjectName()}_velocity",
                velocity
            );
        }
    }
}
```

---

## Storage Options

XRXP supports multiple storage backends that work simultaneously.

### Storage Architecture

```
┌─────────────────────────────────────┐
│          XRXPRecorder               │
└──────────────┬──────────────────────┘
               │
        ┌──────┴──────┐
        │ DataManager │
        └──────┬──────┘
               │
    ┌─────────┼─────────┐
    │         │         │
    ▼         ▼         ▼
┌──────┐ ┌────────┐ ┌──────────┐
│Backup│ │ Remote │ │  Local   │
│(File)│ │ (WebSocket)│ │ (Future) │
└──────┘ └────────┘ └──────────┘
```

### Backup Storage (Local Files)

**Enabled by default** - Cannot be disabled.

- Stores data in compressed JSON files
- Location: `<Application.persistentDataPath>/XRXP/backup/`
- Format: GZip-compressed JSON lines
- Filename: `Trace_yyyy-MM-dd_HH_mm_ss.backup.gz`

**Accessing Backup Files:**

```csharp
// Android
string path = "/storage/emulated/0/Android/data/<package>/files/XRXP/backup/";

// iOS
string path = Application.persistentDataPath + "/XRXP/backup/";

// Windows/Mac
string path = Application.persistentDataPath + "/XRXP/backup/";
```

**Extracting Data:**
```bash
# Decompress
gunzip Trace_2024-03-15_14_30_00.backup.gz

# View content
cat Trace_2024-03-15_14_30_00.backup | jq .
```

### Remote Storage (WebSocket)

Sends data in real-time to a WebSocket server.

**Configuration:**
```csharp
config.OnlineMode = true;
config.WebSocketServer = "ws://your-server.com:8080";
config.AuthorizationToken = "your-api-token"; // Optional
```

**Data Format:**
```json
{
  "Id": "550e8400-e29b-41d4-a716-446655440000",
  "Protocol": "Session",
  "ExperimentId": "MyExperiment",
  "UserId": "user-123",
  "StartDate": 1710507600000,
  "EndDate": 0,
  "Comments": "Test session"
}
```

**Reconnection Logic:**
- Automatic reconnection with exponential backoff
- Queue persists until connection restored
- 3 connection attempts before going offline

### File Upload Server

For media files (images, videos, audio).

**Configuration:**
```csharp
config.FileServer = "http://your-server.com/upload";
config.AuthorizationToken = "your-api-token";
```

**Upload Flow:**
1. Media added via `AddMediaEvent(byte[], ...)`
2. File automatically uploaded to server
3. Server returns URL stored in event

### Local Storage Mode (Not Implemented)

Reserved for future feature to save structured JSON files locally.

---

## Data Types and Events

### Record Types

| Type | Description | Use Case |
|------|-------------|----------|
| **Session** | Experiment session | Start/end of user participation |
| **Environment** | Experimental setup | Room conditions, lighting |
| **User** | Participant info | Demographics, consent |
| **LogEvent** | Semantic events | User actions, system events |
| **InternalEvent** | Raw tracking data | Position, sensor values |
| **Question** | Survey responses | Questionnaires, feedback |
| **MediaEvent** | File references | Screenshots, recordings |
| **Statistic** | Aggregated metrics | Performance scores |

### System Types

**SystemType.WorldPosition**
- Records: `position (Vector3)`, `rotation (Quaternion)`
- Use for: Tracking objects, hands, HMD

**SystemType.QuantitativeValue**
- Records: `value (float)`, `unit (string)`
- Use for: Sensors, biometrics, scores

---

## Code Examples

### Complete Experiment Flow

```csharp
using UnityEngine;
using XRXP;
using XRXP.Recorder;
using XRXP.Recorder.Models;
using System.Collections.Generic;

public class ExperimentController : MonoBehaviour
{
    [Header("Configuration")]
    public string experimentName = "VR_Training_v1";
    public int trialCount = 10;
    
    private string _sessionId;
    private int _currentTrial = 0;
    
    void Start()
    {
        // Wait for XRXP to be ready
        StartCoroutine(InitializeExperiment());
    }
    
    System.Collections.IEnumerator InitializeExperiment()
    {
        // Wait for XRXP initialization
        while (!XRXPManager.IsReady)
        {
            yield return null;
        }
        
        // Start session
        _sessionId = XRXPManager.Recorder.StartSession(
            "Main experiment session",
            "",
            new Dictionary<string, string> {
                { "scene", "TrainingRoom" },
                { "difficulty", "medium" }
            }
        );
        
        Debug.Log($"Session started: {_sessionId}");
        
        // Log experiment start
        XRXPManager.Recorder.AddLogEvent(
            "System", 
            "started", 
            "Experiment",
            new Dictionary<string, string> {
                { "experiment_name", experimentName },
                { "total_trials", trialCount.ToString() }
            }
        );
    }
    
    public void OnTrialComplete(float completionTime, int errors)
    {
        _currentTrial++;
        
        // Log trial result
        XRXPManager.Recorder.AddLogEvent(
            "User",
            "completed",
            $"Trial_{_currentTrial}",
            new Dictionary<string, string> {
                { "completion_time", completionTime.ToString() },
                { "errors", errors.ToString() },
                { "accuracy", ((1f - (float)errors/10f) * 100).ToString() }
            }
        );
        
        // Add performance metric
        XRXPManager.Recorder.AddInternalEvent(
            SystemType.QuantitativeValue,
            "Performance",
            "TrialTime",
            new QuantitativeValue(completionTime, "seconds")
        );
    }
    
    public void OnUserResponse(string questionId, string response)
    {
        XRXPManager.Recorder.AddQuestion(
            questionId,
            response,
            new Dictionary<string, string> {
                { "question_type", "likert" },
                { "response_time", Time.time.ToString() }
            }
        );
    }
    
    public void TakeScreenshot()
    {
        // Capture screenshot
        string filename = $"screenshot_{_sessionId}_{Time.time}.png";
        string path = Application.persistentDataPath + "/" + filename;
        ScreenCapture.CaptureScreenshot(path);
        
        // Log media event
        XRXPManager.Recorder.AddMediaEvent(
            path,
            "image/png",
            $"Trial_{_currentTrial}_Screenshot"
        );
    }
    
    public void EndExperiment()
    {
        // Log experiment end
        XRXPManager.Recorder.AddLogEvent(
            "System",
            "ended",
            "Experiment",
            new Dictionary<string, string> {
                { "trials_completed", _currentTrial.ToString() },
                { "total_duration", Time.time.ToString() }
            }
        );
        
        // Stop session
        XRXPManager.Recorder.StopSession();
        
        // Wait for data transfer
        StartCoroutine(WaitForTransfer());
    }
    
    System.Collections.IEnumerator WaitForTransfer()
    {
        int pending = XRXPManager.Recorder.TransfersState();
        while (pending > 0)
        {
            Debug.Log($"Waiting for {pending} traces to be sent...");
            yield return new WaitForSeconds(1f);
            pending = XRXPManager.Recorder.TransfersState();
        }
        
        // End tracing gracefully
        XRXPManager.Recorder.EndTracing();
        Debug.Log("Experiment data saved successfully!");
    }
    
    void OnApplicationQuit()
    {
        if (XRXPManager.Recorder.isRecording())
        {
            XRXPManager.Recorder.StopSession();
        }
    }
}
```

### Multi-User Session Example

```csharp
public class MultiUserExperiment : MonoBehaviour
{
    public bool isHost = true;
    public string hostSessionId = "";
    
    void Start()
    {
        if (isHost)
        {
            // Host creates session
            hostSessionId = XRXPManager.Recorder.StartSession(
                "Host - Multiplayer Session",
                "host-001"
            );
            
            // Share hostSessionId with other players via network
            NetworkManager.Instance.ShareSessionId(hostSessionId);
        }
        else
        {
            // Guests join existing session
            string sharedSessionId = NetworkManager.Instance.GetSessionId();
            XRXPManager.Recorder.JoinSession(
                sharedSessionId,
                "Guest - Multiplayer Session",
                "guest-001"
            );
        }
    }
}
```

### Error Handling Example

```csharp
public class SafeExperiment : MonoBehaviour
{
    public void SafeLogEvent(string actor, string verb, string obj)
    {
        try
        {
            if (!XRXPManager.IsReady)
            {
                Debug.LogWarning("XRXP not ready yet, buffering event...");
                StartCoroutine(LogWhenReady(actor, verb, obj));
                return;
            }
            
            if (!XRXPManager.Recorder.isRecording())
            {
                Debug.LogWarning("No active session, starting one...");
                XRXPManager.Recorder.StartSession("Auto-started session");
            }
            
            XRXPManager.Recorder.AddLogEvent(actor, verb, obj);
        }
        catch (XRXPException ex)
        {
            Debug.LogError($"XRXP Error: {ex.Message}");
            // Handle gracefully - maybe buffer for later
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Unexpected error: {ex.Message}");
        }
    }
    
    System.Collections.IEnumerator LogWhenReady(string actor, string verb, string obj)
    {
        while (!XRXPManager.IsReady)
        {
            yield return null;
        }
        SafeLogEvent(actor, verb, obj);
    }
}
```

---

## Best Practices

1. **Always check `XRXPManager.IsReady`** before accessing `XRXPManager.Recorder`
2. **Use meaningful session comments** to identify sessions in data
3. **Set appropriate trace frequencies** - not everything needs 90Hz recording
4. **Add environment properties** at session start for context
5. **Handle exceptions gracefully** - don't let recording errors crash your app
6. **Stop sessions properly** to ensure end timestamps are recorded
7. **Monitor `TransfersState()`** before closing application to prevent data loss
8. **Use categories consistently** for easier data analysis (e.g., always "Hand" not "hand" or "Hands")

---

## Troubleshooting

### "Current Scene is not correctly setup for XR Experiments"
**Solution:** Call `XRXP > Setup the scene` from menu or ensure XRXPManager exists in scene.

### Data not appearing on server
**Check:**
- WebSocketServer URL is correct
- OnlineMode is enabled in config
- Check Unity Console for connection errors
- Verify queue with `XRXPManager.Recorder.TransfersState()`

### Missing backup files
**Check:**
- BackUpStorageMode is enabled (default: true)
- Application has file system permissions
- Check correct platform path: `Application.persistentDataPath/XRXP/backup/`

### High memory usage
**Solutions:**
- Reduce trace frequency on XRXPObjectTracker components
- Call `TransfersState()` periodically to monitor queue
- Ensure storage backends are processing data

---

## API Reference Summary

**Main Entry Point:** `XRXPManager.Recorder`

**Session Control:**
- `StartSession()` / `StopSession()` / `JoinSession()`
- `isRecording()` / `GetSessionId()` / `GetUserId()`

**Event Recording:**
- `AddLogEvent()` - Semantic events
- `AddInternalEvent()` - Tracking data
- `AddQuestion()` - Survey responses
- `AddMediaEvent()` - File references

**Metadata:**
- `AddEnvironmentProperties()` / `AddUserProperties()`

**Utility:**
- `TransfersState()` / `EndTracing()`

**Component:**
- `XRXPObjectTracker` - Automatic position tracking

---

For more information, visit: https://espace.science/xrxpdoc/