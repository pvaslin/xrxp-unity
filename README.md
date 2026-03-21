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

- **Eye Tracking** (`com.xrxp.eyetracking`): Oculus eye tracking integration for gaze analysis
- **FrameRate Analyser** (`com.xrxp.framerate`): Real-time performance monitoring with Z-score peak detection
- **Scene Controller** (`com.xrxp.scenecontroller`): WebSocket-based remote scene management

## Installation

### Requirements
- Unity 2021.3 LTS or later
- Target platform: PC, Mac, Linux, or VR headsets (Oculus, etc.)

### Core Package

```bash
# Via Package Manager (Git URL)
https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.core
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.xrxp.core": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.core"
  }
}
```

### Optional Modules

```json
{
  "dependencies": {
    "com.xrxp.core": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.core",
    "com.xrxp.eyetracking": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.eyetracking",
    "com.xrxp.framerate": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.framerate",
    "com.xrxp.scenecontroller": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.scenecontroller"
  }
}
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

## Documentation

- [Getting Started Guide](xrexperiments-package/Packages/com.xrxp.core/Documentation~/GettingStarted.md)
- [Core Concepts](xrexperiments-package/Packages/com.xrxp.core/Documentation~/CoreConcepts.md)
- [Modules Overview](xrexperiments-package/Packages/com.xrxp.core/Documentation~/Modules.md)
- [API Reference](xrexperiments-package/Packages/com.xrxp.core/Documentation~/API_REFERENCE.md)

### Module Documentation

- [Eye Tracking](xrexperiments-package/Packages/com.xrxp.core/Documentation~/EyeTracking.md)
- [FrameRate Analyser](xrexperiments-package/Packages/com.xrxp.core/Documentation~/FrameRateAnalyser.md)
- [Scene Controller](xrexperiments-package/Packages/com.xrxp.core/Documentation~/SceneController.md)
- [Creating Custom Modules](xrexperiments-package/Packages/com.xrxp.core/Documentation~/ModuleCreationGuide.md)

## Project Structure

```
xrxp-unity/
├── Packages/
│   ├── com.xrxp.core/           # Core framework
│   ├── com.xrxp.eyetracking/    # Eye tracking module
│   ├── com.xrxp.framerate/      # Performance monitoring
│   └── com.xrxp.scenecontroller/# Remote scene control
├── AGENTS.md                     # AI agent guidelines
└── README.md                     # This file
```

## Development

### Coding Standards

See [AGENTS.md](AGENTS.md) for:
- Naming conventions
- Code style guidelines
- File organization
- Best practices

### Running Tests

```bash
# All tests
/Applications/Unity/Hub/Editor/2021.3.20f1/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode \
  -testResults $(pwd)/test-results.xml \
  -projectPath $(pwd)/xrexperiments-package
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Merge Request

Please follow the coding standards in [AGENTS.md](AGENTS.md).

## License

[Your License Here]

## Support

- Documentation: See `Documentation~` folders in each package
- Issues: [GitLab Issues](http://gitlab.espace.science/pierre/xrxp-unity/-/issues)
- Discussions: [GitLab Discussions](http://gitlab.espace.science/pierre/xrxp-unity/-/discussions)

## Acknowledgments

- Original modules from [Freemix Framework](https://gitlab.espace.science/pierre/freemix_framework)
- Built for XR research and experimentation
