# XR Experiments (XRXP) for Unity

A comprehensive Unity framework for XR (VR/AR) experimentation and data recording.

## Overview

XRXP (XR Experiments) provides a modular framework for conducting XR research experiments with robust data collection, remote management, and performance monitoring capabilities.

## Features

### Core Framework
- **Session Management**: Start/stop experiment sessions with automatic data organization
- **Data Recording**: Track user actions, object positions, system events, and custom data
- **Multi-storage Support**: Local storage, WebSocket streaming, and file upload capabilities
- **Questionnaires**: Built-in support for experiment questionnaires and surveys

### Available Modules

- **Eye Tracking** (`com.xrxp.module.eyetracking`): Oculus eye tracking integration for gaze analysis
- **FrameRate Analyser** (`com.xrxp.module.framerate`): Real-time performance monitoring with Z-score peak detection
- **Scene Controller** (`com.xrxp.module.scenecontroller`): WebSocket-based remote scene management

## Installation

### Requirements
- Unity 2021.3 LTS or later
- Target platform: PC, Mac, Linux, or VR headsets (Oculus, etc.)

### Core Package

In Unity Package Manager, click `+` > "Add package from git URL" and enter:

```
https://gitlab.espace.science/pierre/xrxp-unity.git?path=Packages/com.xrxp.core
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.xrxp.core": "https://gitlab.espace.science/pierre/xrxp-unity.git?path=Packages/com.xrxp.core"
  }
}
```

### Optional Modules

```json
{
  "dependencies": {
    "com.xrxp.core": "https://gitlab.espace.science/pierre/xrxp-unity.git?path=Packages/com.xrxp.core",
    "com.xrxp.module.eyetracking": "https://gitlab.espace.science/pierre/xrxp-unity.git?path=Packages/com.xrxp.module.eyetracking",
    "com.xrxp.module.framerate": "https://gitlab.espace.science/pierre/xrxp-unity.git?path=Packages/com.xrxp.module.framerate",
    "com.xrxp.module.scenecontroller": "https://gitlab.espace.science/pierre/xrxp-unity.git?path=Packages/com.xrxp.module.scenecontroller"
  }
}
```

### Pin to a Specific Version

Append `#v0.1.0` (or a commit hash) to pin a version:
```
https://gitlab.espace.science/pierre/xrxp-unity.git?path=Packages/com.xrxp.core#v0.1.0
```

## Quick Start

### 1. Setup Scene

```
Menu: XRXP > Setup the scene
```

This creates:
- `XRXPManager` GameObject with core components
- `XRXPConfig` asset in `Assets/XRXPConfig/`

### 2. Configure

Select `XRXPConfig` and configure:
- **Experiment ID**: Unique identifier for your study
- **Online Mode**: Enable WebSocket server connection
- **Server URLs**: WebSocket and file server endpoints

### 3. Record Data

```csharp
// Start session
string sessionId = XRXPManager.Recorder.StartSession("Participant 001");

// Log events
XRXPManager.Recorder.AddLogEvent("User", "clicked", "Button_A");

// Add questionnaire data
XRXPManager.Recorder.AddQuestion("Comfort Level", "High");

// Track objects
// Add XRXPObjectTracker component to any GameObject

// Stop session
XRXPManager.Recorder.StopSession();
```

## Project Structure

```
xrxp-unity/
├── Packages/
│   ├── com.xrxp.core/                  # Core framework
│   ├── com.xrxp.module.eyetracking/    # Eye tracking module
│   ├── com.xrxp.module.framerate/      # Performance monitoring
│   └── com.xrxp.module.scenecontroller/# Remote scene control
├── AGENTS.md                            # AI agent guidelines
├── LICENSE
└── README.md
```

## Documentation

- [Core Package README](Packages/com.xrxp.core/README.md)
- [Full Documentation](https://espace.science/xrxpdoc/)

## Development

### Coding Standards

See [AGENTS.md](AGENTS.md) for naming conventions, code style, and best practices.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Merge Request

## License

[MIT License](LICENSE)

## Support

- Documentation: See `Documentation~` folders in each package
- Issues: [GitLab Issues](https://gitlab.espace.science/pierre/xrxp-unity/-/issues)
