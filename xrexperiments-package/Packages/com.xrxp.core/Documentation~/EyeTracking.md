# Eye Tracking Module

The Eye Tracking module integrates Oculus eye tracking to capture detailed gaze data during XR experiments.

## Overview

This module provides:
- **Eye position tracking**: Record left and right eye positions and rotations
- **Gaze visualization**: Visual debugging with LineRenderers
- **Look area detection**: Track what objects users are looking at
- **Confidence filtering**: Only record high-quality tracking data

## Installation

### Requirements
- Unity 2021.3 LTS or later
- Oculus Integration SDK
- Oculus Quest Pro or other eye-tracking capable headset

### Package Installation

```
Add package from git URL:
https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.eyetracking
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.xrxp.eyetracking": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.eyetracking"
  }
}
```

## Components

### XRXPEyeRecorder

Records continuous eye position and rotation data.

**Location**: `XRXP/Modules/Setup Eye Recorder`

#### Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `TracingEnabled` | bool | Enable/disable recording | true |
| `ConfidenceThreshold` | float (0-1) | Minimum confidence for valid data | 0.5 |
| `lefttmp` | LineRenderer | Visual debugging for left eye | null |
| `righttmp` | LineRenderer | Visual debugging for right eye | null |
| `TraceFrequency` | int | Frames between recordings | 0 (every frame) |

#### Usage

```csharp
// Eye recording starts automatically when:
// 1. XRXP session is active
// 2. Eye tracking is enabled on device
// 3. Confidence is above threshold

// Data is recorded as InternalEvent with SystemType.WorldPosition
// Category: "Eyes"
// Names: "Left Eye", "Right Eye"
```

### XRXPLookAreaRecorder

Detects when users look at specific objects using raycasting.

**Location**: `XRXP/Modules/Setup Eye Tracking Look Area`

#### Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `TracingEnabled` | bool | Enable/disable recording | true |
| `ConfidenceThreshold` | float (0-1) | Minimum confidence for valid data | 0.5 |
| `AreaMask` | LayerMask | Layers to check for look detection | "XRXPArea" layer |

#### Setup

1. Run `XRXP/Modules/Setup Eye Tracking Look Area`
2. This creates a new layer: "XRXPArea"
3. Assign this layer to objects you want to track
4. Add `XRXPLookAreaRecorder` component to XRXPManager

#### Usage

```csharp
// Objects are detected when user looks at them
// Events are logged as:
// Actor: "User"
// Action: "look"
// Value: Object name

// Example output:
// "User look only Cube"
// "User look left Sphere"
// "User look right Button"
```

## Configuration

### Setting Up Visual Debugging

Add LineRenderers to see gaze direction:

```csharp
// Create two LineRenderers in your scene
LineRenderer leftLine = gameObject.AddComponent<LineRenderer>();
LineRenderer rightLine = gameObject.AddComponent<LineRenderer>();

// Assign to eye recorder
XRXPEyeRecorder eyeRecorder = GetComponent<XRXPEyeRecorder>();
eyeRecorder.lefttmp = leftLine;
eyeRecorder.righttmp = rightLine;
```

### Look Area Objects

To track specific objects:

1. Select object in hierarchy
2. Add "XRXPArea" layer (created automatically)
3. Set object's layer to "XRXPArea"
4. Ensure object has a Collider for raycasting

## Data Output

### Eye Position Data

Recorded as `InternalEvent`:
```json
{
  "type": "WorldPosition",
  "category": "Eyes",
  "name": "Left Eye",
  "data": {
    "position": {"x": 0.5, "y": 1.6, "z": -0.2},
    "rotation": {"x": 0, "y": 0, "z": 0, "w": 1}
  }
}
```

### Look Events

Recorded as `LogEvent`:
```json
{
  "actor": "User",
  "action": "look",
  "value": "TargetObjectName",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Best Practices

1. **Confidence Threshold**: Use 0.5 or higher for reliable data
2. **Frame Rate**: Set `TraceFrequency` based on your needs:
   - 0: Every frame (highest detail, higher data volume)
   - 10: 10 times per second (good balance)
   - 30: Once per second (low detail, minimal overhead)
3. **Look Area Layer**: Only assign to objects relevant to your study
4. **Testing**: Always test eye tracking on actual hardware

## Troubleshooting

### Eye tracking not working
- Verify headset supports eye tracking
- Check Oculus Integration is installed
- Enable eye tracking in Oculus settings
- Check `OVRPlugin.eyeTrackingEnabled`

### No data recorded
- Ensure XRXP session is started
- Check confidence is above threshold
- Verify eye tracking is calibrated

### Inaccurate look detection
- Adjust collider sizes on tracked objects
- Check layer mask is set correctly
- Verify raycast distance is appropriate

## API Reference

### XRXPEyeRecorder

```csharp
public class XRXPEyeRecorder : XRXPObjectTracker
{
    public bool EyeTrackingEnabled { get; }
    public float Confidence { get; private set; }
    public float ConfidenceThreshold { get; set; }
    
    public override string GetObjectName();
}
```

### XRXPLookAreaRecorder

```csharp
public class XRXPLookAreaRecorder : MonoBehaviour
{
    public const string DefaultLayerName = "XRXPArea";
    public bool TracingEnabled { get; set; }
    public float ConfidenceThreshold { get; set; }
    public LayerMask AreaMask { get; set; }
}
```

## See Also

- [Modules Overview](./Modules.md)
- [FrameRate Analyser](./FrameRateAnalyser.md)
- [Scene Controller](./SceneController.md)
- [Getting Started Guide](./GettingStarted.md)
