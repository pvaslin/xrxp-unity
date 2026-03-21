# XR Experiments (XRXP) Unity Package Repository

This repository contains the XR Experiments (XRXP) Unity packages following Unity Package Manager (UPM) best practices.

## Repository Structure

```
.
├── Packages/
│   ├── com.xrxp.core/                    # Main package
│   │   ├── Runtime/                      # Runtime scripts
│   │   ├── Editor/                       # Editor scripts
│   │   ├── Tests/                        # Test assemblies
│   │   ├── Samples~/                     # Sample scenes
│   │   ├── Documentation~/               # Documentation
│   │   ├── package.json                  # Package manifest
│   │   ├── CHANGELOG.md
│   │   └── README.md
│   └── com.xrxp.eyetracking/             # Optional eye tracking module
│       ├── Runtime/
│       ├── Editor/
│       ├── Tests/
│       └── package.json
├── LICENSE
└── README.md
```

## Packages

### com.xrxp.core

Core XR Experiments (XRXP) framework for XR experimentation and data recording.

**Features:**
- Session and user management
- Multi-backend data storage (local, remote, backup)
- Event logging and tracking
- Object position/rotation tracking
- XR Integration Toolkit compatible

### com.xrxp.eyetracking

Optional eye tracking module using Oculus Integration.

**Features:**
- Eye gaze tracking
- Look area detection
- Integration with Oculus eye tracking

## Installation

### Via Package Manager

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.xrxp.core": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.core",
    "com.xrxp.eyetracking": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.eyetracking"
  }
}
```

### Development/Local Installation

1. Clone this repository
2. In Unity, open Package Manager
3. Click `+` → "Add package from disk"
4. Select `package.json` from the desired package folder

## Development

### Building

No build step required - Unity Package Manager handles compilation.

### Testing

Tests are located in `Tests/` folders within each package.

### Assembly Definitions

- `XRXP.Runtime` - Runtime assembly
- `XRXP.Editor` - Editor-only assembly
- `XRXP.EyeTracking` - Eye tracking runtime assembly

## Documentation

- [Main Package README](Packages/com.xrxp.core/README.md)
- [Full Documentation](https://espace.science/xrxpdoc/)

## License

[MIT License](LICENSE)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

Please ensure:
- Code follows existing style
- Changes are documented in CHANGELOG.md
- Tests are added/updated as needed