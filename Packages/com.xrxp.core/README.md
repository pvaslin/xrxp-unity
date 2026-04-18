# XR Experiments (XRXP)

A Unity framework for XR experimentation and data recording.

## Overview

XR Experiments (XRXP) is a comprehensive framework designed for conducting experiments in XR (VR/AR) environments. It provides robust data recording capabilities, session management, and extensible modules for various tracking needs.

## Features

- **Session Management**: Create and manage experimental sessions with users and environments
- **Data Recording**: Multiple storage backends (local, remote, backup)
- **Event Tracking**: Log events, questions, media, and statistics
- **Object Tracking**: Track GameObject positions and rotations in world space
- **XR Ready**: Built for Unity XR Interaction Toolkit and Oculus integration

## Installation

### Option 1: Package Manager (Git URL)

1. Open Unity Package Manager (Window > Package Manager)
2. Click the `+` button and select "Add package from git URL"
3. Enter: `https://github.com/pvaslin/xrxp-unity.git?path=/Packages/com.xrxp.core`
4. Click Add

### Option 2: Manual Installation

1. Clone or download this repository
2. Copy the `Packages/com.xrxp.core` folder to your project's `Packages` folder

### Optional Modules

Extend XRXP with optional modules:

- **Eye Tracking**: Oculus eye tracking integration
  ```
  https://github.com/pvaslin/xrxp-unity.git?path=/Packages/com.xrxp.module.eyetracking
  ```

- **FrameRate Analyser**: Performance monitoring and peak detection
  ```
  https://github.com/pvaslin/xrxp-unity.git?path=/Packages/com.xrxp.module.framerate
  ```

- **Scene Controller**: WebSocket-based remote scene management
  ```
  https://github.com/pvaslin/xrxp-unity.git?path=/Packages/com.xrxp.module.scenecontroller
  ```

## Quick Start

1. **Setup Scene**: Go to `XRXP > Setup the scene` in the menu
2. **Configure**: Select the created XRXPConfig asset and set your experiment ID and server details
3. **Start Recording**: Use `XRXPManager.Recorder.StartSession()` to begin
4. **Track Objects**: Add `XRXPObjectTracker` component to GameObjects you want to track

## Usage

```csharp
// Start a session
string sessionId = XRXPManager.Recorder.StartSession("Participant 1");

// Log an event
XRXPManager.Recorder.AddLogEvent("User", "clicked", "Button_A");

// Track object positions automatically with XRXPObjectTracker component

// Stop the session
XRXPManager.Recorder.StopSession();
```

## Documentation

Full documentation is available at: https://xrxp.io/docs/

## Dependencies

- Unity 2021.3.20f1 or later
- TextMeshPro (com.unity.textmeshpro)

### Optional
- Oculus XR Plugin (com.unity.xr.oculus) - for Eye Tracking module

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.