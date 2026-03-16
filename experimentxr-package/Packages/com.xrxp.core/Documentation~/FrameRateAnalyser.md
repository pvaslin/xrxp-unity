# FrameRate Analyser Module

The FrameRate Analyser module monitors application performance in real-time, detecting frame rate spikes and drops during XR experiments.

## Overview

This module provides:
- **Real-time FPS monitoring**: Track frame rate continuously
- **Peak detection**: Automatically identify performance anomalies using statistical analysis
- **Unity Profiler integration**: View metrics in Unity Profiler
- **Event logging**: Log performance events to experiment data

## Installation

### Requirements
- Unity 2021.3 LTS or later
- XRXP Core package

### Package Installation

```
Add package from git URL:
https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.framerate
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.xrxp.framerate": "https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.framerate"
  }
}
```

## How It Works

The module uses the **Z-score algorithm** for real-time peak detection:

1. Maintains a rolling window of frame time values
2. Calculates moving average and standard deviation
3. Detects when current value deviates significantly from the mean
4. Classifies as spike (above) or drop (below)

## Components

### FrameRateMonitor

Main component that monitors frame rate and detects performance issues.

**Location**: `XRXP/Modules/Setup FrameRate Monitor`

#### Properties

| Property | Type | Description | Default | Recommended |
|----------|------|-------------|---------|-------------|
| `Lag` | int | Number of frames in rolling window | 30 | 30-60 |
| `Threshold` | float | Standard deviation multiplier for detection | 5.0 | 3.0-5.0 |
| `Influence` | float | How much peaks affect the baseline (0-1) | 0.0 | 0.0-0.5 |

#### Configuration Guide

**Lag (Window Size)**
- Smaller (10-20): Faster detection, more sensitive to noise
- Medium (30-60): Good balance for most applications
- Larger (100+): Smoother baseline, slower detection

**Threshold**
- Lower (2-3): Detects smaller fluctuations
- Medium (4-5): Detects significant issues only
- Higher (6+): Only detects severe problems

**Influence**
- 0.0: Peaks don't affect baseline (recommended)
- 0.5: Moderate influence
- 1.0: Full influence (peaks become new baseline)

#### Usage

```csharp
// Monitor is added via menu: XRXP/Modules/Setup FrameRate Monitor
// Or manually add FrameRateMonitor component to XRXPManager

FrameRateMonitor monitor = gameObject.AddComponent<FrameRateMonitor>();
monitor.Lag = 30;
monitor.Threshold = 5f;
monitor.Influence = 0f;
```

### PeakDetection

Statistical algorithm for detecting anomalies in time-series data.

```csharp
// Internal class used by FrameRateMonitor
PeakDetection detector = new PeakDetection(lag: 30, threshold: 5f, influence: 0f);

// Returns:
//  1  = Peak detected (above average)
// -1  = Drop detected (below average)
//  0  = Normal
// null = Not enough data yet
int? result = detector.IsPeak(currentFrameTime);
```

### FrameRateStats

Unity Profiler counters for performance metrics.

**Profiler Categories**:
- `Detection of framerate variations` - Count of detected peaks/drops
- `FPS Counter` - Current frame time in milliseconds

View in Unity Profiler: Window → Analysis → Profiler → FrameRate Monitor module

## Data Output

### Performance Events

When a spike or drop is detected during a recording session:

```json
// FPS Spike detected
{
  "actor": "System",
  "action": "detect",
  "value": "a FPS spike",
  "timestamp": "2024-01-15T10:30:00Z"
}

// FPS Drop detected
{
  "actor": "System",
  "action": "detect",
  "value": "a FPS drop",
  "timestamp": "2024-01-15T10:30:01Z"
}
```

### Profiler Data

Two custom counters are available in Unity Profiler:

1. **Detection of framerate variations**
   - Type: Integer counter
   - +1 for each spike
   - -1 for each drop
   - Resets each frame

2. **FPS Counter**
   - Type: Float counter
   - Current frame time in milliseconds
   - Updates every frame

## Use Cases

### VR Comfort Studies
```csharp
// Detect when frame drops might cause discomfort
monitor.Threshold = 3f;  // More sensitive
monitor.Lag = 30;        // 0.5 second window at 60fps

// Log when performance degrades
// Correlate with user comfort ratings
```

### Performance Optimization
```csharp
// Track during development
monitor.Threshold = 4f;
monitor.Lag = 60;  // 1 second window

// Identify which scenes/actions cause drops
// Compare before/after optimization
```

### Automated Testing
```csharp
// Fail test if too many drops
int dropCount = 0;
void Update() {
    if (detectedDrop) dropCount++;
    if (dropCount > 10) {
        Assert.Fail("Too many frame drops");
    }
}
```

## Best Practices

1. **Threshold Tuning**: Start with 5.0, adjust based on your tolerance
2. **Window Size**: Match to your expected event duration
3. **Baseline Establishment**: Allow 1-2 seconds before starting recording
4. **VR Considerations**: Be more sensitive in VR (threshold 3-4)
5. **Data Correlation**: Compare performance events with user actions

## Troubleshooting

### No events logged
- Check XRXP session is active (`XRXPManager.Recorder.isRecording()`)
- Verify component is on XRXPManager GameObject
- Ensure lag window has filled (wait `lag` frames)

### Too many false positives
- Increase threshold (try 6.0 or higher)
- Increase lag window size
- Increase influence to adapt to changing baselines

### Missing real issues
- Decrease threshold (try 3.0)
- Decrease lag window for faster detection
- Decrease influence to maintain stable baseline

### Profiler counters not showing
- Ensure module is imported
- Check "FrameRate Monitor" module is visible in Profiler
- May need to enable "Scripts" category in Profiler

## Performance Considerations

- **CPU Cost**: Minimal (~0.01ms per frame)
- **Memory**: O(n) where n = lag window size
- **GC**: Minimal allocations after initialization
- **Recommended**: Disable in production builds if not needed

## API Reference

### FrameRateMonitor

```csharp
public class FrameRateMonitor : MonoBehaviour
{
    public int Lag;           // Rolling window size
    public float Threshold;   // Detection threshold (std dev multiplier)
    public float Influence;   // Peak influence on baseline (0-1)
    
    void Update();  // Called automatically
}
```

### PeakDetection

```csharp
public class PeakDetection
{
    public PeakDetection(int lag, float threshold, float influence);
    public int? IsPeak(double newValue);
}
```

### FrameRateStats

```csharp
public static class FrameRateStats
{
    public static readonly ProfilerCounterValue<int> PeakSignals;
    public static readonly ProfilerCounterValue<float> FPSCounter;
}
```

## Algorithm Details

The Z-score algorithm:

```
For each new value:
1. Add to rolling window
2. Calculate mean and standard deviation of window
3. Calculate Z-score: (value - mean) / std_dev
4. If |Z-score| > threshold:
   - Z > 0: Spike detected (return 1)
   - Z < 0: Drop detected (return -1)
5. Update filtered values based on influence parameter
6. Return 0 (normal) if no peak
```

## See Also

- [Modules Overview](./Modules.md)
- [Eye Tracking](./EyeTracking.md)
- [Scene Controller](./SceneController.md)
- [Unity Profiler Documentation](https://docs.unity3d.com/Manual/Profiler.html)
