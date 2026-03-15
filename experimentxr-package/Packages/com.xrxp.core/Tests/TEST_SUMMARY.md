# Test Suite Summary for com.xrxp.core

This document provides an overview of all tests created for the XR Experiments (XRXP) package.

## Test Structure

```
Packages/com.xrxp.core/Tests/
├── Runtime/
│   ├── XRXP.Tests.asmdef          # Runtime test assembly definition
│   ├── XRXPExceptionTests.cs      # Exception handling tests
│   ├── UserTests.cs               # User model tests
│   ├── EnvironmentTests.cs        # Environment model tests
│   ├── SessionTests.cs            # Session model tests
│   ├── LogEventTests.cs           # LogEvent model tests
│   ├── MediaEventTests.cs         # MediaEvent model tests
│   ├── QuestionTests.cs           # Question model tests
│   ├── StatisticTests.cs          # Statistic model tests
│   ├── InternalSystemTests.cs     # InternalSystem & InternalEvent tests
│   ├── PropertyTests.cs           # Property classes tests (UserProperty, etc.)
│   └── RecordBaseTests.cs         # Record base classes tests
└── Editor/
    ├── XRXP.Editor.Tests.asmdef   # Editor test assembly definition
    └── EditorMenuTests.cs         # Editor menu and config tests
```

## Test Coverage

### Core Tests (XRXPExceptionTests.cs)
- ✅ Constructor with no parameters
- ✅ Constructor with message (verifies XRXP prefix)
- ✅ Constructor with message and inner exception

### Model Tests

#### UserTests.cs (7 tests)
- ✅ Constructor generates GUID when empty ID provided
- ✅ Constructor uses provided custom ID
- ✅ Constructor with custom protocol
- ✅ Add single user property
- ✅ Add multiple user properties
- ✅ Get properties returns all properties
- ✅ Update value changes property value

#### EnvironmentTests.cs (6 tests)
- ✅ Constructor generates GUID by default
- ✅ Constructor uses provided custom ID
- ✅ Constructor with custom protocol
- ✅ Add single environment property
- ✅ Add multiple environment properties
- ✅ Get properties returns all properties

#### SessionTests.cs (12 tests)
- ✅ Constructor with basic parameters
- ✅ Constructor with comments
- ✅ Constructor with environment sets environment ID
- ✅ Constructor with parent session sets parent ID
- ✅ Constructor with custom ID
- ✅ Constructor with null ID generates new ID
- ✅ Update end date changes protocol to updateenddate
- ✅ Add internal system to session
- ✅ Try get existing internal system returns true
- ✅ Try get non-existing internal system returns false
- ✅ Get environment returns environment object
- ✅ Get user information returns user object

#### LogEventTests.cs (7 tests)
- ✅ Constructor with required parameters
- ✅ Constructor with duration sets duration
- ✅ Constructor with custom protocol
- ✅ Add single event property
- ✅ Add multiple event properties
- ✅ Get properties returns all properties

#### MediaEventTests.cs (6 tests)
- ✅ Constructor with required parameters
- ✅ Constructor with duration
- ✅ Constructor with custom protocol
- ✅ Set file path stores path
- ✅ Get file path before setting returns null

#### QuestionTests.cs (7 tests)
- ✅ Constructor with required parameters
- ✅ Constructor with custom protocol
- ✅ Update answer changes answer
- ✅ Add single question property
- ✅ Add multiple question properties
- ✅ Get properties returns all properties

#### StatisticTests.cs (1 test)
- ✅ Constructor with required parameters

#### InternalSystemTests.cs (6 tests)
- InternalSystem:
  - ✅ Constructor with required parameters
  - ✅ Constructor with custom protocol
  - ✅ System type is serialized to string
- InternalEvent:
  - ✅ Constructor with required parameters
  - ✅ Constructor with custom protocol
  - ✅ Value is serialized to JSON

#### PropertyTests.cs (9 tests)
- UserProperty (2 tests)
- EnvironmentProperty (1 test)
- LogEventProperty (1 test)
- QuestionProperty (1 test)
- Plus additional property-specific tests

#### RecordBaseTests.cs (4 tests)
- ✅ RecordBase default values
- ✅ RecordBase property setters
- ✅ RecordWithProperties inheritance
- ✅ GetProperties returns list

### Editor Tests (EditorMenuTests.cs)
- ✅ SetupConfig creates config asset
- ✅ SetupConfig creates config with default values
- ✅ XRXPConfig is ScriptableObject
- ✅ XRXPConfig default values are set

## Running Tests

### All Runtime Tests
```bash
/Applications/Unity/Hub/Editor/2021.3.20f1/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode \
  -testResults $(pwd)/test-results.xml \
  -projectPath $(pwd)
```

### Specific Test Class
```bash
/Applications/Unity/Hub/Editor/2021.3.20f1/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode \
  -testFilter "XRXP.Tests.SessionTests" \
  -testResults $(pwd)/test-results.xml \
  -projectPath $(pwd)
```

### Single Test Method
```bash
/Applications/Unity/Hub/Editor/2021.3.20f1/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode \
  -testFilter "XRXP.Tests.SessionTests.Constructor_WithBasicParams_CreatesSession" \
  -testResults $(pwd)/test-results.xml \
  -projectPath $(pwd)
```

## Total Test Count

- **Runtime Tests**: ~60 tests across 11 test classes
- **Editor Tests**: 4 tests across 2 test classes
- **Total**: ~64 tests

## Notes

- Tests use NUnit framework (standard for Unity testing)
- Tests are organized by model/class being tested
- Each test follows Arrange-Act-Assert pattern
- Setup methods create common test fixtures
- TearDown methods clean up assets where needed
- Tests verify both happy paths and edge cases
