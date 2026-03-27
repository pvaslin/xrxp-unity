# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-03-27

Initial release of XR Experiments (XRXP) for Unity.

### Core

- **XRXPManager** singleton with automatic lifecycle management and cancellation token support
- **XRXPConfig** ScriptableObject for experiment settings (WebSocket server, file server, auth token, online/backup modes)
- **XRXPRecorder** public API for session management, event logging, and transfer monitoring

### Recording & Data Models

- Session management with nested session support and multi-user join
- Event types: LogEvent, InternalEvent, MediaEvent, Question, Statistic
- User and Environment models with extensible properties
- XRXPObjectTracker for automatic transform tracking

### Storage Pipeline

- **StorageBase** abstract class with dedicated consumer threads and BlockingCollection for zero-spin queue processing
- **RemoteStorage** — WebSocket client with automatic reconnection and graceful close
- **BackupStorage** — GZip-compressed JSON lines with per-session media file backup
- **FileUploadStorage** — HTTP multipart file upload with retry and cancellation support
- **BackupStorageSync** — synchronous backup variant for immediate writes
- **SerializedRecord** envelope for thread-safe, pre-serialized record dispatch
- **BackupResender** — reads previous backup files and resends records and media to remote on demand; renames to `.sent` after success
- Main-thread serialization with background I/O for full decoupling from the game loop

### Exchange System

- **XRXPExchangeManager** for bidirectional WebSocket communication with a dashboard
- **ExchangeModality** ScriptableObject defining status and control field schemas
- **[ExchangeControl]** attribute for automatic method discovery and command dispatch
- Inspector-based event bindings for control fields
- **XRXPRecorderExchangeBridge** for dashboard-triggered backup resend and live transfer status reporting

### Developer API

- `RemainingCount` / `IsAllSent` — simple checks for pending data before quit
- `ResendBackups()` / `IsResending` — trigger and monitor backup replay
- `PendingBackupFileCount()` — count unsent backup files from previous sessions
- `TransfersState()` — live queue depth across all storage backends

### Modules

- **com.xrxp.module.eyetracking** — optional eye tracking data capture
- **com.xrxp.module.framerate** — frame rate analysis and reporting
- **com.xrxp.module.scenecontroller** — scene management via Exchange controls

### Editor

- Scene setup menu for quick project configuration
- Configuration editor for XRXPConfig assets
- Assembly Definition files for proper code organization
