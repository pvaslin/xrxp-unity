# XRXP FrameRate Analyser

Real-time frame rate monitoring and performance analysis module for XRXP Unity framework.

## Features

- **Real-time FPS monitoring**: Track frame rate continuously during experiments
- **Automatic peak detection**: Uses Z-score algorithm to detect FPS spikes and drops
- **Unity Profiler integration**: Custom profiler counters for performance metrics
- **Event logging**: Automatically logs performance events to XRXP session data

## Installation

### Requirements
- Unity 2021.3 LTS or later
- XRXP Core package

### Package Manager

Add via git URL:
```
https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.framerate
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.xrxp.module.framerate": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.framerate"
  }
}
```

## Quick Start

1. Setup XRXP in your scene (XRXP > Setup the scene)
2. Add FrameRate Monitor: `XRXP > Modules > Setup FrameRate Monitor`
3. Configure settings in the XRXPManager inspector:
   - **Lag**: Rolling window size (default: 30)
   - **Threshold**: Detection sensitivity (default: 5.0)
   - **Influence**: Peak influence on baseline (default: 0.0)
4. Start your XRXP session
5. View real-time data in Unity Profiler > FrameRate Monitor

## Documentation

See the [full documentation](../com.xrxp.core/Documentation~/FrameRateAnalyser.md) for:
- Detailed configuration options
- Algorithm explanation
- Use cases and best practices
- API reference

## How It Works

The module uses the Z-score statistical algorithm to detect frame rate anomalies:

1. Maintains a rolling window of frame times
2. Calculates moving average and standard deviation
3. Detects when current frame time deviates significantly
4. Classifies as spike (faster) or drop (slower)
5. Logs events to XRXP session data

## Data Output

When performance issues are detected:

```json
{
  "actor": "System",
  "action": "detect",
  "value": "a FPS spike"  // or "a FPS drop"
}
```

## Dependencies

- `com.xrxp.core` (automatically resolved)

## License

See main XRXP repository for license information.
