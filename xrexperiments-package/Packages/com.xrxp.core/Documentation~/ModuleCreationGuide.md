# Module Creation Guide

This guide walks you through creating a new XRXP module, following best practices and project conventions.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Step-by-Step Creation](#step-by-step-creation)
- [Documentation Requirements](#documentation-requirements)
- [Code Standards](#code-standards)
- [Publishing Checklist](#publishing-checklist)
- [Example Module](#example-module)

## Overview

An XRXP module is a Unity package that extends the core XRXP framework with specific functionality. Modules are:

- **Optional**: Users choose which modules to install
- **Self-contained**: Each module is an independent package
- **Consistent**: Follow naming conventions and architecture
- **Documented**: Include comprehensive documentation

## Prerequisites

Before creating a module:

1. **Unity 2021.3 LTS** or later
2. **XRXP Core** package installed
3. Understanding of Unity Package Manager
4. Familiarity with XRXP architecture (see [Core Concepts](CoreConcepts.md))

## Step-by-Step Creation

### 1. Create Package Structure

Create the directory structure:

```
Packages/
└── com.xrxp.module.modulename/
    ├── Runtime/
    │   └── XRXP.ModuleName.asmdef
    ├── Editor/
    │   └── XRXP.ModuleName.Editor.asmdef
    ├── Tests/
    │   ├── Runtime/
    │   └── Editor/
    ├── package.json
    └── README.md
```

### 2. Configure Package Manifest

Create `package.json`:

```json
{
    "name": "com.xrxp.module.modulename",
    "displayName": "XRXP Module Name",
    "version": "1.0.0",
    "unity": "2021.3",
    "description": "Brief description of what this module does",
    "keywords": [
        "xr",
        "vr",
        "keyword1",
        "keyword2"
    ],
    "category": "XR",
    "dependencies": {
        "com.xrxp.core": "1.0.0"
    }
}
```

**Naming Convention**:
- Package name: `com.xrxp.module.lowercase`
- Display name: `XRXP PascalCase`

### 3. Create Assembly Definitions

**Runtime Assembly** (`Runtime/XRXP.ModuleName.asmdef`):

```json
{
    "name": "XRXP.ModuleName",
    "rootNamespace": "XRXP.Modules.ModuleName",
    "references": [
        "XRXP.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**Editor Assembly** (`Editor/XRXP.ModuleName.Editor.asmdef`):

```json
{
    "name": "XRXP.ModuleName.Editor",
    "rootNamespace": "XRXP.Modules.ModuleName.Editor",
    "references": [
        "XRXP.Runtime",
        "XRXP.ModuleName"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### 4. Implement Core Functionality

Create your main component in the Runtime folder:

```csharp
using UnityEngine;

namespace XRXP.Modules.ModuleName
{
    /// <summary>
    /// Brief description of the component
    /// </summary>
    public class XRXPModuleName : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Description of this field")]
        public bool EnableFeature = true;
        
        [Tooltip("Description of this field")]
        public float SomeValue = 1.0f;
        
        private void Start()
        {
            // Initialize
        }
        
        private void Update()
        {
            // Main logic
            if (XRXPManager.IsReady && XRXPManager.Recorder.isRecording())
            {
                RecordData();
            }
        }
        
        private void RecordData()
        {
            // Record to XRXP
            XRXPManager.Recorder.AddLogEvent("ModuleName", "action", "value");
        }
    }
}
```

### 5. Add Editor Integration

Create menu items in the Editor folder:

```csharp
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XRXP.Modules.ModuleName;

namespace XRXP.Modules.ModuleName.Editor
{
    public class ModuleNameMenu : MonoBehaviour
    {
        [MenuItem("XRXP/Modules/Setup Module Name", false, 10)]
        public static void SetupModule()
        {
            GameObject gm = GameObject.Find("XRXPManager");
            if (gm == null)
            {
                Debug.LogError("XRXPManager not found. Run 'XRXP/Setup the scene' first.");
                return;
            }
            
            if (gm.GetComponent<XRXPModuleName>() == null)
            {
                XRXPModuleName component = gm.AddComponent<XRXPModuleName>();
                // Set default values
                component.EnableFeature = true;
                component.SomeValue = 1.0f;
            }
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("Module Name setup complete!");
        }
    }
}
```

### 6. Create Documentation

Create comprehensive documentation following the template in [Documentation Template](#documentation-template).

Save as: `Packages/com.xrxp.core/Documentation~/ModuleName.md`

### 7. Update Table of Contents

Add your module to `TableOfContents.md`:

```markdown
- [Modules](Modules.md)
  - [Module Name](ModuleName.md)
    - [Installation](ModuleName.md#installation)
    - [Usage](ModuleName.md#usage)
```

## Documentation Requirements

Every module must include:

### Required Sections

1. **Overview** - What the module does
2. **Installation** - How to install
3. **Components** - Main classes/components
4. **Configuration** - How to configure
5. **Data Output** - What data is recorded
6. **Best Practices** - Usage recommendations
7. **Troubleshooting** - Common issues
8. **API Reference** - Public API documentation

### Documentation Template

```markdown
# Module Name

## Overview

One-paragraph description of the module.

Key features:
- Feature 1
- Feature 2
- Feature 3

## Installation

### Requirements
- Unity 2021.3 LTS or later
- XRXP Core package
- Any additional requirements

### Package Installation

```
Add package from git URL:
https://github.com/yourorg/xrxp.git?path=/Packages/com.xrxp.module.modulename
```

## Components

### ComponentName

Brief description.

**Location**: `XRXP/Modules/Setup Module Name`

#### Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `Property1` | bool | Description | true |
| `Property2` | float | Description | 1.0 |

#### Usage

```csharp
// Code example
```

## Configuration

How to configure the module.

## Data Output

What data is recorded and in what format.

## Best Practices

1. Best practice 1
2. Best practice 2
3. Best practice 3

## Troubleshooting

### Issue 1
Solution...

### Issue 2
Solution...

## API Reference

```csharp
public class ComponentName : MonoBehaviour
{
    public Type Property1;
    public Type Property2;
    
    public void Method();
}
```

## See Also

- [Modules Overview](./Modules.md)
- [Other related modules]
```

## Code Standards

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | `XRXP.Modules.ModuleName` | `XRXP.Modules.EyeTracking` |
| Classes | PascalCase | `XRXPEyeRecorder` |
| Public Methods | PascalCase | `StartRecording()` |
| Private Fields | camelCase with _ | `_recorder` |
| Public Fields | camelCase | `tracingEnabled` |
| Constants | PascalCase | `DefaultLayerName` |

### Code Structure

```csharp
using System;
using UnityEngine;
using XRXP.Recorder.Models;

namespace XRXP.Modules.ModuleName
{
    /// <summary>
    /// XML documentation for the class
    /// </summary>
    public class XRXPModuleName : MonoBehaviour
    {
        // Private fields first
        private string _privateField;
        
        // Public fields second
        [Header("Category")]
        public string PublicField;
        
        // Properties third
        public string Property { get; private set; }
        
        // Unity methods fourth
        private void Awake() { }
        private void Start() { }
        private void Update() { }
        
        // Public methods fifth
        public void PublicMethod() { }
        
        // Private methods last
        private void PrivateMethod() { }
    }
}
```

### Best Practices

1. **Namespace**: Always use `XRXP.Modules.YourModuleName`
2. **Menu Items**: Place under `XRXP/Modules/`
3. **XRXP Integration**: Check `XRXPManager.IsReady` before recording
4. **Error Handling**: Use `XRXPException` for module errors
5. **Comments**: Use XML documentation for public APIs
6. **Null Checks**: Validate GameObject.Find results
7. **Editor Only**: Use Editor assembly for menu items

## Publishing Checklist

Before publishing your module:

### Code
- [ ] All code compiles without errors
- [ ] No compiler warnings
- [ ] XML documentation for public APIs
- [ ] Menu items work correctly
- [ ] Integration with XRXPManager tested

### Testing
- [ ] Tested in Unity Editor
- [ ] Tested in standalone build
- [ ] Tested on target platform (VR headset, etc.)
- [ ] Edge cases handled
- [ ] Error scenarios tested

### Documentation
- [ ] README.md in package folder
- [ ] Module documentation in `Documentation~/`
- [ ] TableOfContents.md updated
- [ ] Modules.md updated
- [ ] Code examples tested

### Package
- [ ] package.json valid
- [ ] Correct dependencies listed
- [ ] Assembly definitions correct
- [ ] Folder structure follows convention
- [ ] Version number appropriate

### Repository
- [ ] Added to main repository
- [ ] Git tags for versions
- [ ] CHANGELOG.md updated
- [ ] License file included

## Example Module

Here's a complete example of a simple tracking module:

### File Structure

```
com.xrxp.module.inputtracker/
├── Runtime/
│   ├── XRXP.InputTracker.asmdef
│   └── XRXPInputTracker.cs
├── Editor/
│   ├── XRXP.InputTracker.Editor.asmdef
│   └── InputTrackerMenu.cs
├── package.json
└── README.md
```

### package.json

```json
{
    "name": "com.xrxp.module.inputtracker",
    "displayName": "XRXP Input Tracker",
    "version": "1.0.0",
    "unity": "2021.3",
    "description": "Tracks user input (button presses, controller actions) during XR experiments",
    "keywords": ["xr", "vr", "input", "controller"],
    "category": "XR",
    "dependencies": {
        "com.xrxp.core": "1.0.0"
    }
}
```

### XRXPInputTracker.cs

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace XRXP.Modules.InputTracker
{
    /// <summary>
    /// Tracks controller input during XR experiments
    /// </summary>
    public class XRXPInputTracker : MonoBehaviour
    {
        [Header("Configuration")]
        public bool TrackButtonPresses = true;
        public bool TrackTriggerValues = true;
        
        [Header("Controller References")]
        public InputActionReference leftTrigger;
        public InputActionReference rightTrigger;
        public InputActionReference leftButton;
        public InputActionReference rightButton;
        
        private void OnEnable()
        {
            if (leftButton != null)
                leftButton.action.performed += OnLeftButtonPressed;
            if (rightButton != null)
                rightButton.action.performed += OnRightButtonPressed;
        }
        
        private void OnDisable()
        {
            if (leftButton != null)
                leftButton.action.performed -= OnLeftButtonPressed;
            if (rightButton != null)
                rightButton.action.performed -= OnRightButtonPressed;
        }
        
        private void Update()
        {
            if (!XRXPManager.IsReady || !XRXPManager.Recorder.isRecording())
                return;
                
            if (TrackTriggerValues)
            {
                RecordTriggerValues();
            }
        }
        
        private void RecordTriggerValues()
        {
            if (leftTrigger != null)
            {
                float value = leftTrigger.action.ReadValue<float>();
                if (value > 0.1f)
                {
                    XRXPManager.Recorder.AddLogEvent("Input", "leftTrigger", value.ToString());
                }
            }
        }
        
        private void OnLeftButtonPressed(InputAction.CallbackContext context)
        {
            if (XRXPManager.IsReady && XRXPManager.Recorder.isRecording())
            {
                XRXPManager.Recorder.AddLogEvent("Input", "buttonPressed", "LeftButton");
            }
        }
        
        private void OnRightButtonPressed(InputAction.CallbackContext context)
        {
            if (XRXPManager.IsReady && XRXPManager.Recorder.isRecording())
            {
                XRXPManager.Recorder.AddLogEvent("Input", "buttonPressed", "RightButton");
            }
        }
    }
}
```

### InputTrackerMenu.cs

```csharp
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XRXP.Modules.InputTracker;

namespace XRXP.Modules.InputTracker.Editor
{
    public class InputTrackerMenu : MonoBehaviour
    {
        [MenuItem("XRXP/Modules/Setup Input Tracker", false, 10)]
        public static void SetupInputTracker()
        {
            GameObject gm = GameObject.Find("XRXPManager");
            if (gm == null)
            {
                Debug.LogError("XRXPManager not found. Run 'XRXP/Setup the scene' first.");
                return;
            }
            
            if (gm.GetComponent<XRXPInputTracker>() == null)
            {
                XRXPInputTracker tracker = gm.AddComponent<XRXPInputTracker>();
                tracker.TrackButtonPresses = true;
                tracker.TrackTriggerValues = true;
            }
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("Input Tracker setup complete!");
        }
    }
}
```

### ModuleName.md (Documentation)

Create following the documentation template above.

## Next Steps

After creating your module:

1. **Test thoroughly** in various scenarios
2. **Create documentation** following the template
3. **Submit for review** if part of team project
4. **Publish** to your repository
5. **Announce** to users with changelog

## Questions?

For questions about module creation:
- Review existing modules as examples
- Check [Modules Overview](./Modules.md) for conventions
- See [API Reference](API_REFERENCE.md) for XRXP APIs

## See Also

- [Modules Overview](./Modules.md)
- [Eye Tracking Module](./EyeTracking.md)
- [FrameRate Analyser Module](./FrameRateAnalyser.md)
- [Scene Controller Module](./SceneController.md)
