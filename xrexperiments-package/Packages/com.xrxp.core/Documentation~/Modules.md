# XRXP Modules

XRXP Modules are optional extensions that add specific functionality to the core XRXP framework. Each module is delivered as a separate Unity package, allowing you to include only the features you need.

## Available Modules

### [Eye Tracking](./EyeTracking.md)
**Package**: `com.xrxp.module.eyetracking`

Integrates Oculus eye tracking to record:
- Left and right eye positions and rotations
- Eye gaze direction and confidence levels
- Look area detection (what objects the user is looking at)

**Best for**: Experiments requiring gaze analysis, attention tracking, or foveated rendering research.

[Learn more about Eye Tracking →](./EyeTracking.md)

---

### [FrameRate Analyser](./FrameRateAnalyser.md)
**Package**: `com.xrxp.module.framerate`

Monitors application performance with:
- Real-time FPS monitoring
- Automatic spike and drop detection using Z-score algorithm
- Unity Profiler integration with custom counters
- Performance event logging

**Best for**: Performance analysis, identifying frame drops in VR experiences, optimizing rendering.

[Learn more about FrameRate Analyser →](./FrameRateAnalyser.md)

---

### [Scene Controller](./SceneController.md)
**Package**: `com.xrxp.module.scenecontroller`

Enables remote experiment management via WebSocket:
- Remote scene switching from a control server
- Real-time scene state notifications
- WebSocket-based communication protocol
- Support for multi-scene experiments

**Best for**: Remote experiment control, automated testing, distributed experiments.

[Learn more about Scene Controller →](./SceneController.md)

---

## Module Architecture

All XRXP modules follow a consistent architecture:

```
Module Package/
├── Runtime/           # Runtime scripts
│   ├── ModuleName.cs
│   └── ...
├── Editor/           # Editor-only scripts
│   ├── ModuleMenu.cs
│   └── ...
├── package.json      # Package manifest
└── Documentation~/   # Module documentation (optional)
```

### Key Principles

1. **Namespace Convention**: All modules use `XRXP.Modules.ModuleName` namespace
2. **Assembly Definitions**: Separate Runtime and Editor assemblies
3. **Menu Integration**: Setup commands under `XRXP/Modules/` menu
4. **Dependency**: All modules depend on `com.xrxp.core`
5. **Optional**: Modules are completely optional - core XRXP works standalone

## Installation

### Via Package Manager

1. Open Unity Package Manager (Window > Package Manager)
2. Click `+` → "Add package from git URL"
3. Enter the module URL:
   - Eye Tracking: `https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.eyetracking`
   - FrameRate: `https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.framerate`
   - Scene Controller: `https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.scenecontroller`

### Dependencies

Modules automatically resolve their dependencies:
- All modules → `com.xrxp.core`
- Eye Tracking → `Unity.XR.Oculus`

## Creating Custom Modules

Want to create your own XRXP module? Check out our [Module Creation Guide](./ModuleCreationGuide.md) for:

- Step-by-step module setup
- Coding standards and conventions
- Documentation requirements
- Publishing guidelines

---

## Quick Reference

| Module | Purpose | Key Classes |
|--------|---------|-------------|
| Eye Tracking | Record eye movements | `XRXPEyeRecorder`, `XRXPLookAreaRecorder` |
| FrameRate Analyser | Performance monitoring | `FrameRateMonitor`, `PeakDetection` |
| Scene Controller | Remote scene management | `XRXPSceneController`, `Message` |

## Support

For module-specific issues, refer to the individual module documentation pages linked above.

For general XRXP support, see the [Getting Started Guide](./GettingStarted.md) or [Core Concepts](./CoreConcepts.md).
