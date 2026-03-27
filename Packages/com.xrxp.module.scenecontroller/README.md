# XRXP Scene Controller

WebSocket-based remote scene management module for XRXP Unity framework.

## Features

- **Remote scene switching**: Control scene transitions from a WebSocket server
- **Real-time notifications**: Broadcast scene state changes to server
- **Scene validation**: Only allow transitions to scenes in Build Settings
- **Unity Events**: Hook custom logic to scene changes
- **Multi-scene support**: Manage complex experiments across multiple scenes

## Installation

### Requirements
- Unity 2021.3 LTS or later
- XRXP Core package
- WebSocket server implementation

### Package Manager

Add via git URL:
```
https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.scenecontroller
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.xrxp.module.scenecontroller": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.scenecontroller"
  }
}
```

## Quick Start

1. Setup XRXP in your scene (XRXP > Setup the scene)
2. Add scenes to Build Settings (File > Build Settings)
3. Add Scene Controller: `XRXP > Modules > Setup Scene Controller`
4. Configure Service URL in inspector (e.g., `ws://localhost:8080`)
5. Implement WebSocket server (see documentation for examples)
6. Start your XRXP session

## Documentation

See the [full documentation](../com.xrxp.core/Documentation~/SceneController.md) for:
- WebSocket protocol specification
- Server implementation examples (Node.js, Python)
- Configuration options
- Security considerations
- API reference

## Communication Protocol

### Connection URL
```
ws://your-server:port/link/{deviceUniqueIdentifier}
```

### Message Format
```json
{
  "protocol": "changeScene",
  "properties": {
    "scene": "TargetSceneName"
  }
}
```

## Use Cases

- **Remote experiment control**: Researchers control flow from server
- **Multi-participant studies**: Synchronize groups across scenes
- **Automated testing**: Programmatic scene switching for tests
- **Distributed experiments**: Central control of multiple locations

## Dependencies

- `com.xrxp.core` (automatically resolved)

## License

See main XRXP repository for license information.
