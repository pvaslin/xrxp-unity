# Getting Started with XR Experiments (XRXP)

## Installation

### Via Package Manager (Git URL)

1. Open Unity Package Manager (Window > Package Manager)
2. Click the `+` button → "Add package from git URL"
3. Enter: `https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.core`
4. Click Add

### Manual Installation

1. Download or clone this repository
2. Copy the `Packages/com.xrxp.core` folder to your project's `Packages` folder

## Quick Start

### 1. Setup Your Scene

Go to menu: **XRXP > Setup the scene**

This creates:
- An `XRXP` GameObject with XRXPManager component
- A `XRXPConfig` asset in `Assets/XRXPConfig/`

### 2. Configure

Select the created `XRXPConfig` asset and configure:
- **Experiment ID**: Unique identifier for your experiment
- **Online Mode**: Enable/disable WebSocket server connection
- **WebSocket Server**: Your server URL (e.g., `ws://localhost:8080`)
- **File Server**: URL for file uploads
- **Authorization Token**: API token for authentication

### 3. Start Recording

```csharp
// Start a recording session
string sessionId = XRXPManager.Recorder.StartSession("Participant 001");

// Log events
XRXPManager.Recorder.AddLogEvent("User", "clicked", "Button_A");

// Ask questions
XRXPManager.Recorder.AddQuestion("Comfort Level", "High");

// Stop the session
XRXPManager.Recorder.StopSession();
```

### 4. Track Objects

Add the `XRXPObjectTracker` component to any GameObject you want to track:

1. Select a GameObject in your scene
2. Add Component → XRXPObjectTracker
3. Configure:
   - **Category**: Type of object (e.g., "Body")
   - **Object Name**: Name of object (e.g., "Hand", "Head", "Prop")
   - **Trace Frequency**: How often to record (0 = every frame)

The object's position and rotation will be automatically recorded during sessions.

## Next Steps

- Read about [Core Concepts](CoreConcepts.md)
- Learn about different [Event Types](Tracking.md)
- Check the [API Reference](API.md)