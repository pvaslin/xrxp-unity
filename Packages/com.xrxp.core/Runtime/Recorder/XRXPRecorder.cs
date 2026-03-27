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
        /// <param name="environmentProperties">Properties who describe the environnement specification</param>
        /// <param name="environmentId">the environnement Id of the environnement who is already set in the database</param>
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
        /// Join a multi-user exercise in an environnement
        /// </summary>
        /// <param name="sessionId">The Session Id of the Session you join</param>
        /// <param name="comments">Use to describe the role of the user who joins the multi player session</param>
        /// <param name="userId">Set manually the user ID. By default the userID is a UUID</param>
        /// <param name="environmentId">the environnement Id of the environnement who is already set up by one the other participant in the exercise</param>
        /// <param name="environmentProperties">Properties who describe the environnement specification</param>
        public void JoinSession(string sessionId, string comments, string userId = "", Dictionary<string, string> environmentProperties = null, string environmentId = "")
        {
            this._dataManager.StartSession(comments, userId, environmentProperties, environmentId, sessionId);
        }

        /// <summary>
        /// Get the current environnement ID
        /// </summary>
        /// <returns>Environnement ID</returns>
        public string GetEnvironnementId()
        {
            return this._dataManager.GetEnvironnementId();
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
        public bool isRecording()
        {
            return this._dataManager.isRecording();
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
        /// Create a answer to the question
        /// </summary>
        /// <param name="label">Label of the question</param>
        /// <param name="answer">Answer of the question</param>
        /// <param name="properties">Properties to enhance the answer</param>
        public void AddQuestion(string label, string answer, Dictionary<string, string> properties = null)
        {
            this._dataManager.AddQuestion(label, answer, properties);
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
        /// Indicate the number of remaining traces to send/store
        /// </summary>
        public int TransfersState()
        {
            return this._dataManager.RemainingTraceCount();
        }

        /// <summary>
        /// Indicate to XR Experiments Trace module to stop the services of transfers after sending all traces remaining
        /// </summary>
        public async void EndTracing()
        {
            await this._dataManager.SafeDispose();
            // this._dataManager.Dispose();
        }

        public void QuitApplication()
        {
            this._dataManager.Dispose();
        }

    }
}
