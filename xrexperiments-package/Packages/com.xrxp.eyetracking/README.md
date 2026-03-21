# XRXP Eye Tracking

Oculus eye tracking integration module for XRXP Unity framework.

## Features

- **Eye position tracking**: Record left and right eye positions and rotations
- **Gaze visualization**: Debug eye direction with LineRenderers
- **Look area detection**: Track what objects users are looking at
- **Confidence filtering**: Only record high-quality tracking data
- **Oculus Integration**: Works with Oculus Quest Pro and compatible headsets

## Installation

### Requirements
- Unity 2021.3 LTS or later
- XRXP Core package
- Oculus Integration SDK
- Eye-tracking capable headset (Quest Pro, etc.)

### Package Manager

Add via git URL:
```
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

## Quick Start

1. Setup XRXP in your scene (XRXP > Setup the scene)
2. Add Eye Recorder: `XRXP > Modules > Setup Eye Recorder`
3. (Optional) Add Look Area tracking: `XRXP > Modules > Setup Eye Tracking Look Area`
4. Assign "XRXPArea" layer to objects you want to track
5. Start your XRXP session

## Components

### XRXPEyeRecorder
Records continuous eye position and rotation data.

### XRXPLookAreaRecorder
Detects when users look at specific objects using raycasting.

## Documentation

See the [full documentation](../com.xrxp.core/Documentation~/EyeTracking.md) for:
- Detailed setup instructions
- Configuration options
- Data output format
- Best practices and troubleshooting
- API reference

## Data Output

### Eye Position
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
```json
{
  "actor": "User",
  "action": "look",
  "value": "TargetObjectName"
}
```

## Dependencies

- `com.xrxp.core` (automatically resolved)
- `Unity.XR.Oculus` (Oculus Integration)

## License

See main XRXP repository for license information.
