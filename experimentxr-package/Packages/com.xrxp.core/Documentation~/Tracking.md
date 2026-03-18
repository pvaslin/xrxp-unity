# Tracking

XRXP provides comprehensive tracking capabilities for objects, events, and user interactions.

## Object Tracking

Track GameObject positions and rotations automatically during recording sessions.

### XRXPObjectTracker

The `XRXPObjectTracker` component records position and rotation data for any GameObject.

#### Setup

**Method 1: Component Inspector**
1. Select GameObject in hierarchy
2. Add Component → XRXPObjectTracker
3. Configure in Inspector

**Method 2: Code**
```csharp
XRXPObjectTracker tracker = gameObject.AddComponent<XRXPObjectTracker>();
tracker.Category = "Body";
tracker.ObjectName = "RightHand";
tracer.TraceFrequency = 10; // Record every 10 frames
```

#### Configuration

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `TracingEnabled` | bool | Enable/disable recording | true |
| `Category` | string | Object category (e.g., "Body", "Prop") | "" |
| `ObjectName` | string | Object identifier | GameObject.name |
| `TraceFrequency` | int | Frames between recordings (0=every frame) | 0 |

#### TraceFrequency Guide

| Value | Recording Rate | Use Case |
|-------|----------------|----------|
| 0 | Every frame | Detailed movement analysis |
| 10 | 6 times/sec (at 60fps) | Smooth tracking, moderate data |
| 30 | 2 times/sec | Position updates only |
| 60 | 1 time/sec | Infrequent position checks |

#### Categories

Common category names:
- `Body` - Body parts (hands, head, feet)
- `Prop` - Manipulable objects
- `Environment` - Static scene elements
- `UI` - User interface elements
- `Agent` - AI or other characters

#### Example Usage

```csharp
// Track player hands
XRXPObjectTracker leftHand = leftHandGO.AddComponent<XRXPObjectTracker>();
leftHand.Category = "Body";
leftHand.ObjectName = "LeftHand";
leftHand.TraceFrequency = 5;

XRXPObjectTracker rightHand = rightHandGO.AddComponent<XRXPObjectTracker>();
rightHand.Category = "Body";
rightHand.ObjectName = "RightHand";
rightHand.TraceFrequency = 5;

// Track interactable objects
foreach (GameObject obj in interactableObjects)
{
    XRXPObjectTracker tracker = obj.AddComponent<XRXPObjectTracker>();
    tracker.Category = "Prop";
    tracker.ObjectName = obj.name;
    tracker.TraceFrequency = 10;
}
```

#### Data Output

Object tracking produces `InternalEvent` records:

```json
{
  "type": "WorldPosition",
  "timestamp": "2024-01-15T10:30:00Z",
  "category": "Body",
  "name": "RightHand",
  "data": {
    "position": {"x": 0.5, "y": 1.2, "z": -0.3},
    "rotation": {"x": 0.0, "y": 0.7, "z": 0.0, "w": 0.7}
  }
}
```

#### Custom Tracking

Extend `XRXPObjectTracker` for custom behavior:

```csharp
public class CustomTracker : XRXPObjectTracker
{
    public float VelocityThreshold = 0.1f;
    private Vector3 _lastPosition;
    
    protected override void Record()
    {
        // Only record if moving fast enough
        float velocity = (transform.position - _lastPosition).magnitude / Time.deltaTime;
        if (velocity > VelocityThreshold)
        {
            base.Record();
        }
        _lastPosition = transform.position;
    }
}
```

---

## Events

Events record discrete actions and occurrences during experiments.

### Event Types

XRXP supports several event types for different use cases:

#### 1. Log Events

General-purpose events with actor-action-value structure.

```csharp
void AddLogEvent(string actor, string action, string value)
```

**Use for**:
- User interactions (clicks, grabs, selections)
- State changes (started, completed, failed)
- System events (loaded, saved, error)
- Milestones (level complete, task done)

**Examples**:
```csharp
// User actions
XRXPManager.Recorder.AddLogEvent("User", "clicked", "StartButton");
XRXPManager.Recorder.AddLogEvent("User", "grabbed", "Sword");
XRXPManager.Recorder.AddLogEvent("User", "released", "Sword");
XRXPManager.Recorder.AddLogEvent("User", "looked_at", "NPC_Character");

// System events
XRXPManager.Recorder.AddLogEvent("System", "level_loaded", "Level_3");
XRXPManager.Recorder.AddLogEvent("System", "checkpoint_reached", "Checkpoint_A");
XRXPManager.Recorder.AddLogEvent("System", "error", "Network timeout");

// Custom actors
XRXPManager.Recorder.AddLogEvent("NPC_Guide", "spoke", "Welcome message");
XRXPManager.Recorder.AddLogEvent("GameManager", "state_changed", "Playing");
```

#### 2. Questions

Questionnaire responses and survey answers.

```csharp
void AddQuestion(string name, string value)
```

**Use for**:
- Post-experiment surveys
- In-experience ratings
- Presence/immersion scales
- Comfort/sickness assessments
- Demographics

**Examples**:
```csharp
// Simulator sickness questionnaire
XRXPManager.Recorder.AddQuestion("SSQ_General", "2");
XRXPManager.Recorder.AddQuestion("SSQ_Nausea", "1");
XRXPManager.Recorder.AddQuestion("SSQ_Oculomotor", "3");
XRXPManager.Recorder.AddQuestion("SSQ_Disorientation", "2");

// Presence questionnaire
XRXPManager.Recorder.AddQuestion("IPQ_Spatial", "High");
XRXPManager.Recorder.AddQuestion("IPQ_Involvement", "Medium");
XRXPManager.Recorder.AddQuestion("IPQ_Experienced_Realism", "High");

// Custom ratings
XRXPManager.Recorder.AddQuestion("Comfort_Level", "7");
XRXPManager.Recorder.AddQuestion("Task_Difficulty", "Medium");
XRXPManager.Recorder.AddQuestion("Would_Recommend", "Yes");
```

#### 3. Statistics

Numerical measurements and metrics.

```csharp
void AddStatistic(string name, float value)
```

**Use for**:
- Performance metrics (FPS, latency)
- Completion times
- Distances traveled
- Error rates
- Custom measurements

**Examples**:
```csharp
// Performance
XRXPManager.Recorder.AddStatistic("Average_FPS", 88.5f);
XRXPManager.Recorder.AddStatistic("Min_FPS", 72.0f);
XRXPManager.Recorder.AddStatistic("Frame_Drops", 3.0f);

// Task metrics
XRXPManager.Recorder.AddStatistic("Completion_Time", 245.3f);
XRXPManager.Recorder.AddStatistic("Errors_Made", 2.0f);
XRXPManager.Recorder.AddStatistic("Object_Distance_Traveled", 15.7f);

// VR specific
XRXPManager.Recorder.AddStatistic("Play_Area_Size", 12.5f);
XRXPManager.Recorder.AddStatistic("Head_Rotation_Total", 720.0f);
```

#### 4. Media Events

Media playback tracking.

```csharp
void AddMediaEvent(string type, string name, string action, long timestamp)
```

**Use for**:
- Video playback
- Audio playback
- Tutorial sequences
- Instructional content

**Examples**:
```csharp
long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

// Video events
XRXPManager.Recorder.AddMediaEvent("Video", "Tutorial_Intro", "started", now);
XRXPManager.Recorder.AddMediaEvent("Video", "Tutorial_Intro", "paused", now + 30000);
XRXPManager.Recorder.AddMediaEvent("Video", "Tutorial_Intro", "resumed", now + 35000);
XRXPManager.Recorder.AddMediaEvent("Video", "Tutorial_Intro", "completed", now + 120000);

// Audio events
XRXPManager.Recorder.AddMediaEvent("Audio", "Background_Music", "started", now);
XRXPManager.Recorder.AddMediaEvent("Audio", "Voice_Over", "started", now + 5000);
```

### Event Naming Conventions

#### Actors

Be consistent with actor naming:

```csharp
// Good - consistent, descriptive
XRXPManager.Recorder.AddLogEvent("User", "clicked", "button");
XRXPManager.Recorder.AddLogEvent("User", "grabbed", "object");

// Good - multiple users
XRXPManager.Recorder.AddLogEvent("User_001", "clicked", "button");
XRXPManager.Recorder.AddLogEvent("User_002", "clicked", "button");

// Good - specific entities
XRXPManager.Recorder.AddLogEvent("NPC_Guide", "spoke", "welcome");
XRXPManager.Recorder.AddLogEvent("System", "loaded", "level");

// Avoid - inconsistent
XRXPManager.Recorder.AddLogEvent("player", "clicked", "button");
XRXPManager.Recorder.AddLogEvent("User", "clicked", "button");
XRXPManager.Recorder.AddLogEvent("USER", "clicked", "button");
```

#### Actions

Use verb-based actions:

```csharp
// Good - clear verbs
XRXPManager.Recorder.AddLogEvent("User", "clicked", "button");
XRXPManager.Recorder.AddLogEvent("User", "grabbed", "object");
XRXPManager.Recorder.AddLogEvent("User", "released", "object");
XRXPManager.Recorder.AddLogEvent("User", "looked_at", "target");
XRXPManager.Recorder.AddLogEvent("User", "entered", "zone");
XRXPManager.Recorder.AddLogEvent("User", "exited", "zone");
XRXPManager.Recorder.AddLogEvent("System", "started", "timer");
XRXPManager.Recorder.AddLogEvent("System", "completed", "task");
XRXPManager.Recorder.AddLogEvent("System", "failed", "connection");

// Avoid - unclear or nouns
XRXPManager.Recorder.AddLogEvent("User", "click", "button");
XRXPManager.Recorder.AddLogEvent("User", "button_press", "button");
XRXPManager.Recorder.AddLogEvent("User", "action", "button");
```

#### Values

Keep values concise but descriptive:

```csharp
// Good - specific identifiers
XRXPManager.Recorder.AddLogEvent("User", "clicked", "StartButton");
XRXPManager.Recorder.AddLogEvent("User", "grabbed", "RedCube_01");
XRXPManager.Recorder.AddLogEvent("User", "entered", "TutorialZone");

// Good - state values
XRXPManager.Recorder.AddLogEvent("System", "state_changed", "Playing");
XRXPManager.Recorder.AddLogEvent("System", "state_changed", "Paused");
XRXPManager.Recorder.AddLogEvent("System", "state_changed", "Completed");

// Avoid - vague or overly long
XRXPManager.Recorder.AddLogEvent("User", "clicked", "button");
XRXPManager.Recorder.AddLogEvent("User", "clicked", "the button on the left side of the screen");
```

### Event Timing

#### When to Record

Record events at meaningful moments:

```csharp
// User Actions - record on interaction
void OnPointerClick()
{
    XRXPManager.Recorder.AddLogEvent("User", "clicked", gameObject.name);
}

// State Changes - record on transition
void OnStateChanged(GameState newState)
{
    XRXPManager.Recorder.AddLogEvent("System", "state_changed", newState.ToString());
}

// Task Completion - record on finish
void OnTaskComplete(Task task)
{
    XRXPManager.Recorder.AddLogEvent("Task", "completed", task.name);
    XRXPManager.Recorder.AddStatistic($"Task_{task.name}_Time", task.Duration);
}
```

#### Batching Events

For high-frequency events, consider batching or sampling:

```csharp
// Bad - recording every frame
void Update()
{
    XRXPManager.Recorder.AddLogEvent("User", "position", transform.position.ToString()); // Too much!
}

// Good - record only when significant change occurs
private Vector3 _lastRecordedPosition;
void Update()
{
    if (Vector3.Distance(transform.position, _lastRecordedPosition) > 0.1f)
    {
        // Use object tracker instead for position
        _lastRecordedPosition = transform.position;
    }
}

// Good - record discrete events only
void OnTriggerEnter(Collider other)
{
    XRXPManager.Recorder.AddLogEvent("User", "entered", other.name);
}

void OnTriggerExit(Collider other)
{
    XRXPManager.Recorder.AddLogEvent("User", "exited", other.name);
}
```

---

## Data Export

### File Locations

Recorded data is saved to:

```
Application.persistentDataPath/
└── XRXP/
    └── {ExperimentId}/
        └── {SessionId}/
            ├── session.json
            └── traces/
                └── trace_001.json
```

### Data Structure

Each session generates:

```json
{
  "session": {
    "id": "uuid",
    "experimentId": "Experiment_001",
    "userId": "uuid",
    "startTime": "2024-01-15T10:30:00Z",
    "endTime": "2024-01-15T10:45:00Z",
    "comments": "VR Comfort Study"
  },
  "events": [...],
  "internalEvents": [...],
  "questions": [...],
  "statistics": [...],
  "mediaEvents": [...]
}
```

---

## Best Practices

### 1. Plan Your Schema

Define naming conventions before starting:

```csharp
public static class EventSchema
{
    public static class Actors
    {
        public const string User = "User";
        public const string System = "System";
        public const string NPC = "NPC";
    }
    
    public static class Actions
    {
        public const string Clicked = "clicked";
        public const string Grabbed = "grabbed";
        public const string Released = "released";
        public const string Entered = "entered";
        public const string Exited = "exited";
    }
}

// Usage
XRXPManager.Recorder.AddLogEvent(
    EventSchema.Actors.User, 
    EventSchema.Actions.Clicked, 
    "StartButton"
);
```

### 2. Validate Session State

Always check if recording before adding data:

```csharp
void RecordEvent(string actor, string action, string value)
{
    if (XRXPManager.IsReady && XRXPManager.Recorder.isRecording())
    {
        XRXPManager.Recorder.AddLogEvent(actor, action, value);
    }
}
```

### 3. Use Appropriate Event Types

Choose the right event type for your data:

```csharp
// Log events for actions
XRXPManager.Recorder.AddLogEvent("User", "clicked", "Button");

// Questions for survey data
XRXPManager.Recorder.AddQuestion("Comfort_Rating", "7");

// Statistics for numerical data
XRXPManager.Recorder.AddStatistic("Completion_Time", 120.5f);

// Media events for audio/video
XRXPManager.Recorder.AddMediaEvent("Video", "Tutorial", "started", timestamp);
```

### 4. Clean Up

Ensure proper cleanup when done:

```csharp
void OnApplicationQuit()
{
    if (XRXPManager.IsReady)
    {
        // Stop session cleanly
        XRXPManager.Recorder.StopSession();
        
        // Wait for uploads
        XRXPManager.Recorder.EndTracing();
    }
}
```

---

## Troubleshooting

### Events Not Recording

1. Check if session is started:
   ```csharp
   if (!XRXPManager.Recorder.isRecording())
   {
       Debug.LogWarning("No active session!");
   }
   ```

2. Verify XRXPManager exists:
   ```csharp
   if (XRXPManager.IsReady == false)
   {
       Debug.LogError("XRXP not initialized!");
   }
   ```

3. Check for exceptions:
   ```csharp
   try
   {
       XRXPManager.Recorder.AddLogEvent("Test", "test", "test");
   }
   catch (Exception ex)
   {
       Debug.LogError($"Recording failed: {ex.Message}");
   }
   ```

### Data Not Appearing

1. Check file locations in `Application.persistentDataPath`
2. Verify write permissions
3. Check disk space
4. Review Unity console for errors

### Too Much Data

1. Increase `TraceFrequency` on trackers
2. Filter events (only record significant ones)
3. Use batching for high-frequency data
4. Shorten session duration

---

## See Also

- [Getting Started](GettingStarted.md) - Quick start guide
- [Core Concepts](CoreConcepts.md) - Understanding XRXP architecture
- [API Reference](API_REFERENCE.md) - Complete API documentation
- [Modules](Modules.md) - Optional tracking modules
