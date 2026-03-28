using System;
using System.Collections.Generic;
using System.Threading;
using XRXP.Recorder.Models;

namespace XRXP.Recorder
{
    public class XRXPRecorder
    {
        private DataManager _dataManager;
        private XRXPConfig _config;

        public XRXPRecorder(XRXPConfig config, CancellationToken cancellation)
        {
            this._config = config;
            this._dataManager = new DataManager(
                cancellation,
                this._config.ExperimentID,
                this._config.LocalStorageMode,
                this._config.BackUpStorageMode,
                (this._config.OnlineMode) ? new Uri(this._config.WebSocketServer) : null,
                (this._config.FileServer.Length > 0) ? new Uri(this._config.FileServer) : null,
                (this._config.AuthorizationToken.Length > 0) ? this._config.AuthorizationToken : null);
        }


        /// <summary>
        /// Start a person's handover
        /// </summary>
        /// <param name="userId">Set manually the user ID. By default the userID is a UUID</param>
        /// <param name="comments">Use to describe the role of the user who joins the multi player session</param>
        /// <param name="environmentProperties">Properties who describe the environment specification</param>
        /// <param name="environmentId">the environment Id of the environment who is already set in the database</param>
        /// <returns></returns>
        public string StartSession(string comments = "", string userId = "", Dictionary<string, string> environmentProperties = null, string environmentId = null)
        {
            return this._dataManager.StartSession(comments, userId, environmentProperties, environmentId);
        }

        /// <summary>
        /// Stop the latest started session currently executed
        /// </summary>
        public void StopSession()
        {
            this._dataManager.StopSession();
        }

        /// <summary>
        /// Join a multi-user exercise in an environment
        /// </summary>
        /// <param name="sessionId">The Session Id of the Session you join</param>
        /// <param name="comments">Use to describe the role of the user who joins the multi player session</param>
        /// <param name="userId">Set manually the user ID. By default the userID is a UUID</param>
        /// <param name="environmentId">the environment Id of the environment who is already set up by one the other participant in the exercise</param>
        /// <param name="environmentProperties">Properties who describe the environment specification</param>
        public void JoinSession(string sessionId, string comments, string userId = "", Dictionary<string, string> environmentProperties = null, string environmentId = "")
        {
            this._dataManager.StartSession(comments, userId, environmentProperties, environmentId, sessionId);
        }

        /// <summary>
        /// Get the current environment ID
        /// </summary>
        /// <returns>Environment ID</returns>
        public string GetEnvironmentId()
        {
            return this._dataManager.GetEnvironmentId();
        }

        /// <summary>
        /// Get the current session ID
        /// </summary>
        /// <returns>Session ID</returns>
        public string GetSessionId()
        {
            return this._dataManager.GetSessionId();
        }
        /// <summary>
        /// Get the current User ID
        /// </summary>
        /// <returns>User ID</returns>
        public string GetUserId()
        {
            return this._dataManager.GetUserId();
        }

        /// <summary>
        /// Indicate if a Session is started
        /// </summary>
        /// <returns></returns>
        public bool IsRecording()
        {
            return this._dataManager.IsRecording();
        }

        /// <summary>
        /// Create an internal event
        /// </summary>
        /// <param name="systemType">Type of data value added</param>
        /// <param name="systemName">Name of the internal system (e.g. HMD trackers)</param>
        /// <param name="property">Property of the system (e.g. Left Controller, Right Controller, Head)</param>
        /// <param name="value">Value</param>
        public void AddInternalEvent(SystemType systemType, string systemName, string property, Jsonable value)
        {
            this._dataManager.AddInternalEvent(systemType, systemName, property, value);
        }

        /// <summary>
        /// Log an event
        /// </summary>
        /// <param name="actor">Actor of the event</param>
        /// <param name="verb">Action realize during the event</param>
        /// <param name="object">Object of the event</param>
        /// <param name="properties">Dictionary of properties link to the event</param>
        public void AddLogEvent(string actor, string verb, string @object, Dictionary<string, string> properties = null)
        {
            this._dataManager.AddLogEvent(actor, verb, @object, null, properties);
        }

        /// <summary>
        /// Create a event containing a media (audio, image, video etc.)
        /// </summary>
        /// <param name="filePath">Path of the file to trace</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="name">Name of the event</param>
        public void AddMediaEvent(string filePath, string mimeType, string name, int duration = 0)
        {
            this._dataManager.AddMediaEvent(filePath, mimeType, name, duration);
        }

        /// <summary>
        /// Create a event containing a media (audio, image, video etc.)
        /// </summary>
        /// <param name="bytes">Media to trace</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="name">Name of the event</param>
        public void AddMediaEvent(byte[] bytes, string mimeType, string name, int duration = 0)
        {
            this._dataManager.AddMediaEvent(bytes, mimeType, name, duration);
        }

        /// <summary>
        /// Create an answer to a question (backwards compatible, generates random questionId)
        /// </summary>
        /// <param name="label">Label of the question</param>
        /// <param name="answer">Answer of the question</param>
        /// <param name="properties">Properties to enhance the answer</param>
        public void AddQuestion(string label, string answer, Dictionary<string, string> properties = null)
        {
            this._dataManager.AddQuestion(label, answer, properties);
        }

        /// <summary>
        /// Create an answer to a specific question with developer-defined ID for grouping answers across sessions/users
        /// </summary>
        /// <param name="questionId">Developer-defined identifier for grouping answers to the same question</param>
        /// <param name="label">Question text/label</param>
        /// <param name="answer">Answer text</param>
        /// <param name="properties">Optional metadata</param>
        public void AddQuestion(string questionId, string label, string answer, Dictionary<string, string> properties = null)
        {
            this._dataManager.AddQuestion(questionId, label, answer, "", null, properties);
        }

        /// <summary>
        /// Create an answer with user context
        /// </summary>
        /// <param name="questionId">Developer-defined identifier for grouping answers to the same question</param>
        /// <param name="label">Question text/label</param>
        /// <param name="answer">Answer text</param>
        /// <param name="userId">User ID to associate with this answer</param>
        /// <param name="properties">Optional metadata</param>
        public void AddQuestion(string questionId, string label, string answer, string userId, Dictionary<string, string> properties = null)
        {
            this._dataManager.AddQuestion(questionId, label, answer, userId, null, properties);
        }

        /// <summary>
        /// Create a standalone question answer (no session link)
        /// </summary>
        /// <param name="questionId">Developer-defined identifier for grouping answers to the same question</param>
        /// <param name="label">Question text/label</param>
        /// <param name="answer">Answer text</param>
        /// <param name="userId">User ID to associate with this answer</param>
        /// <param name="properties">Optional metadata</param>
        public void AddStandaloneQuestion(string questionId, string label, string answer, string userId, Dictionary<string, string> properties = null)
        {
            this._dataManager.AddStandaloneQuestion(questionId, label, answer, userId, properties);
        }

        /// <summary>
        /// Add environment properties on the environment of the active scene
        /// </summary>
        /// <param name="properties">Properties to enhance the answer</param>
        public void AddEnvironmentProperties(Dictionary<string, string> properties = null)
        {
            this._dataManager.AddEnvironmentProperties(properties);
        }

        /// <summary>
        /// Add user properties on th user of the active scene
        /// </summary>
        /// <param name="properties">Properties of the user</param>
        public void AddUserProperties(Dictionary<string, string> properties = null)
        {
            this._dataManager.AddUserProperties(properties);
        }

        /// <summary>
        /// Returns the total number of records and files still waiting to be sent or stored.
        /// This includes both the live queue (current session) and unsent backup files from previous sessions.
        /// Use this to check if it is safe to quit the application.
        /// </summary>
        public int RemainingCount => this._dataManager.RemainingTraceCount() + this._dataManager.PendingBackupFileCount();

        /// <summary>
        /// Returns true if all data has been sent (live queue empty and no pending backups).
        /// </summary>
        public bool IsAllSent => RemainingCount == 0 && !IsResending;

        /// <summary>
        /// Indicate the number of remaining traces to send/store in the live queue.
        /// </summary>
        public int TransfersState()
        {
            return this._dataManager.RemainingTraceCount();
        }

        /// <summary>
        /// Returns the number of unsent backup files (.backup.gz) from previous sessions.
        /// </summary>
        public int PendingBackupFileCount()
        {
            return this._dataManager.PendingBackupFileCount();
        }

        /// <summary>
        /// Resend all backup data (records and media files) to the remote server.
        /// Runs on a background thread. Check IsResending for progress.
        /// </summary>
        public void ResendBackups()
        {
            this._dataManager.ResendBackups();
        }

        /// <summary>
        /// Indicates if a backup resend operation is currently in progress.
        /// </summary>
        public bool IsResending => this._dataManager.IsResending;

        /// <summary>
        /// Stop the recording services. Drains all remaining records before closing connections.
        /// </summary>
        public void EndTracing()
        {
            this._dataManager.Dispose();
        }

        public void QuitApplication()
        {
            this._dataManager.Dispose();
        }
    }
}
