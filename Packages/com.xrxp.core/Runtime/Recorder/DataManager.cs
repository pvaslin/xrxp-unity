using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using MimeTypes;
using XRXP.Recorder.Storage;
using XRXP.Recorder.Models;
using System.Threading;

namespace XRXP.Recorder
{
    public class DataManager
    {
        private string _experimentId;
        private User _user;
        private Stack<Session> _currentSessions;
        private bool _servicesStarted = false;
        private List<IDataStorage> _storages;
        private RemoteStorage _remoteStorage;
        private FileUploadStorage _fileUploadStorage;
        private BackupResender _backupResender;
        private string _workDirectory;
        private string _backupDirectory;
        private CancellationToken _cancellationToken;

        public bool IsResending => _backupResender != null && _backupResender.IsRunning;

        public DataManager(
            CancellationToken cancellationToken,
            string experimentId,
            bool localStorageMode = false,
            bool backUpStorageMode = true,
            Uri webSocketServer = null,
            Uri fileServer = null,
            string authorizationToken = null)
        {
            this._experimentId = experimentId;
            this._workDirectory = $"{Application.persistentDataPath}/XRXP";
            this._backupDirectory = $"{this._workDirectory}/backup";
            this._storages = new List<IDataStorage>();
            this._currentSessions = new Stack<Session>();
            this._cancellationToken = cancellationToken;
            if (backUpStorageMode)
            {
                this._storages.Add(new BackupStorage(this._workDirectory));
            }
            if (webSocketServer != null)
            {
                this._remoteStorage = new RemoteStorage(webSocketServer, authorizationToken);
                this._storages.Add(this._remoteStorage);
            }
            if (fileServer != null)
            {
                this._fileUploadStorage = new FileUploadStorage(fileServer, authorizationToken);
                this._storages.Add(this._fileUploadStorage);
            }
            if (this._remoteStorage != null)
            {
                this._backupResender = new BackupResender(
                    this._backupDirectory,
                    this._remoteStorage,
                    this._fileUploadStorage,
                    this._cancellationToken);
            }
            if (localStorageMode)
            {
                // TODO need to be implemented
            }
        }

        private void LaunchServices()
        {
            if (this._servicesStarted)
            {
                return;
            }
            this._servicesStarted = true;
            foreach (IDataStorage storage in this._storages)
            {
                Debug.Log("Storage " + storage.GetType().ToString());
                storage.Open(this._cancellationToken);
            }
        }

        /// <summary>
        /// Serialize a record on the main thread and fan out to all storage backends.
        /// Handles RecordWithProperties by recursively serializing sub-records.
        /// </summary>
        private void SendTrace(RecordBase trace)
        {
            string json = JsonUtility.ToJson(trace);
            string filePath = (trace is MediaEvent media) ? media.GetFilePath() : null;
            var serialized = new SerializedRecord(trace.Id, json, filePath);

            foreach (IDataStorage storage in this._storages)
            {
                storage.Add(serialized);
            }

            if (trace is RecordWithProperties rwp)
            {
                foreach (var property in rwp.GetProperties())
                {
                    SendTrace(property);
                }
            }
        }

        public string StartSession(string comments, string userId = "", Dictionary<string, string> environmentProperties = null, string environmentId = null, string sessionId = null)
        {
            if (!this._servicesStarted)
            {
                this.LaunchServices();
            }
            this._user = new User(userId);
            Session parent;
            if (!this._currentSessions.TryPeek(out parent))
            {
                parent = null;
            }
            Models.Environment env;
            if (environmentId != null && environmentId.Length > 0)
            {
                env = new Models.Environment(environmentId);
            }
            else
            {
                env = new Models.Environment();
            }
            if (environmentProperties != null)
            {
                env.AddEnvironmentProperties(environmentProperties);
            }
            this.SendTrace(env);

            Session newSession = new Session(sessionId, this._experimentId, this._user, this.GetTime(), comments, env, parent);
            this._currentSessions.Push(newSession);

            this.SendTrace(newSession);
            return newSession.Id;
        }

        public void StopSession()
        {
            Session session;
            if (this._currentSessions.TryPop(out session))
            {
                session.UpdateEndDate(this.GetTime());
                this.SendTrace(session);
            }
            else
            {
                throw new XRXPException("There is no Session to stop.");
            }
        }

        public string GetUserId()
        {
            if (this._user != null)
            {
                return this._user.Id;
            }
            else
            {
                throw new XRXPException("Cannot get the User Id, there is no current user, please start a session which create a new user.");
            }
        }

        public string GetEnvironmentId()
        {
            Session session;
            if (this._currentSessions.TryPeek(out session))
            {
                return session.EnvironmentId;
            }
            else
            {
                throw new XRXPException("Cannot get the Environment Id, there is no started Session.");
            }
        }

        public void AddEnvironmentProperties(Dictionary<string,string> properties)
        {
            Session session;
            if (this._currentSessions.TryPeek(out session))
            {
                session.GetEnvironment().AddEnvironmentProperties(properties);
                this.SendTrace(session.GetEnvironment());
            }
            else
            {
                throw new XRXPException("Cannot add Environment properties, there is no started Session.");
            }
        }

        public string GetSessionId()
        {
            Session session;
            if (this._currentSessions.TryPeek(out session))
            {
                return session.Id;
            }
            else
            {
                throw new XRXPException("Cannot get the current session Id, there is no started Session.");
            }
        }

        public void AddInternalEvent(SystemType systemType, string systemName, string property, Jsonable value)
        {
            Session session;
            if (!this._currentSessions.TryPeek(out session))
            {
                throw new XRXPException("Cannot add an InternalEvent, there is no started Session.");
            }
            InternalSystem internalSystem;
            if (!session.TryGetInternalSystem(systemName, out internalSystem))
            {
                internalSystem = new InternalSystem(systemName, systemType, this._user, session);
                this.SendTrace(internalSystem);
                session.AddInternalSystem(internalSystem);
            }
            InternalEvent ie = new InternalEvent(this.GetTime(), property, value, internalSystem);
            this.SendTrace(ie);
        }

        public void AddUserProperties(Dictionary<string, string> properties)
        {
            if (this._user == null)
            {
                throw new XRXPException("The User is not defined. Please start a Trial to create the User.");
            }
            this._user.AddUserProperties(properties);
            this.SendTrace(this._user);
        }

        public void AddUserProperty(string property, string value)
        {
            if (this._user == null)
            {
                throw new XRXPException("The User is not defined. Please start a Trial to create the User.");
            }
            this._user.AddUserProperty(property, value);
            this.SendTrace(this._user);
        }

        public void AddQuestion(string label, string answer, Dictionary<string, string> properties = null)
        {
            this.AddQuestion(Guid.NewGuid().ToString(), label, answer, "", null, properties);
        }

        public void AddQuestion(string questionId, string label, string answer, string userId = "", Session session = null, Dictionary<string, string> properties = null)
        {
            Session targetSession = session;
            if (targetSession == null && this._currentSessions.Count > 0)
            {
                this._currentSessions.TryPeek(out targetSession);
            }
            string targetUserId = userId;
            if (string.IsNullOrEmpty(targetUserId) && this._user != null)
            {
                targetUserId = this._user.Id;
            }
            Question question = new Question(questionId, label, answer, targetSession, targetUserId);
            if (properties != null)
            {
                question.AddQuestionProperties(properties);
            }
            this.SendTrace(question);
        }

        public void AddStandaloneQuestion(string questionId, string label, string answer, string userId, Dictionary<string, string> properties = null)
        {
            Question question = new Question(questionId, label, answer, null, userId);
            if (properties != null)
            {
                question.AddQuestionProperties(properties);
            }
            this.SendTrace(question);
        }

        public void AddLogEvent(string actor, string verb, string @object, int? duration = null, Dictionary<string, string> properties = null)
        {
            Session session;
            if (!this._currentSessions.TryPeek(out session))
            {
                throw new XRXPException("Cannot add an LogEvent, there is no started Session.");
            }
            LogEvent logEvent = new LogEvent(this.GetTime(), actor, verb, @object, this._user, session, duration);
            if (properties != null)
            {
                logEvent.AddEventProperties(properties);
            }
            this.SendTrace(logEvent);
        }

        public void AddMediaEvent(string filePath, string mimeType, string name, int duration = 0)
        {
            Session session;
            if (!this._currentSessions.TryPeek(out session))
            {
                throw new XRXPException("Cannot add an MediaEvent, there is no started session.");
            }
            MediaEvent media = new MediaEvent(this.GetTime(), mimeType, name, this._user, session, duration);

            // Copy the file on a background thread, then serialize and send on completion
            Task.Run(() =>
            {
                Uri file = new Uri(filePath);
                if (file.IsFile && File.Exists(file.AbsolutePath))
                {
                    String mediaDirectory = $"{this._backupDirectory}/sessions/{session.Id}/medias";
                    if (!Directory.Exists(mediaDirectory))
                    {
                        Directory.CreateDirectory(mediaDirectory);
                    }
                    string cpFilePath = $"{mediaDirectory}/{media.Id}{Path.GetExtension(file.AbsolutePath)}";
                    File.Copy(file.AbsolutePath, cpFilePath);

                    media.SetFilePath(cpFilePath);
                    // Serialize here — JsonUtility.ToJson is safe from background threads in Unity 2020+
                    // but the record is fully constructed at this point
                    string json = JsonUtility.ToJson(media);
                    var serialized = new SerializedRecord(media.Id, json, cpFilePath);
                    foreach (IDataStorage storage in this._storages)
                    {
                        storage.Add(serialized);
                    }
                }
                else
                {
                    throw new XRXPException($"The file is not accessible ({filePath})");
                }
            }, this._cancellationToken);
        }

        public void AddMediaEvent(byte[] bytes, string mimeType, string name, int duration = 0)
        {
            Session session;
            if (!this._currentSessions.TryPeek(out session))
            {
                throw new XRXPException("Cannot add an MediaEvent, there is no started session.");
            }
            MediaEvent media = new MediaEvent(this.GetTime(), mimeType, name, this._user, session, duration);

            // Save the binary on a background thread, then serialize and send once file path is set
            Task.Run(() =>
            {
                string mediaDirectory = $"{this._backupDirectory}/sessions/{session.Id}/medias";
                if (!Directory.Exists(mediaDirectory))
                {
                    Directory.CreateDirectory(mediaDirectory);
                }
                string savedFilePath = $"{mediaDirectory}/{media.Id}{MimeTypeMap.GetExtension(mimeType)}";
                try
                {
                    FileStream fs = File.Create(savedFilePath);
                    fs.Write(bytes);
                    fs.Dispose();
                }
                catch (Exception)
                {
                    throw new XRXPException($"The file is not accessible ({savedFilePath})");
                }
                media.SetFilePath(savedFilePath);
                string json = JsonUtility.ToJson(media);
                var serialized = new SerializedRecord(media.Id, json, savedFilePath);
                foreach (IDataStorage storage in this._storages)
                {
                    storage.Add(serialized);
                }
            }, this._cancellationToken);
        }

        public void AddSessionStatistic(string name, string value, string parameters = null)
        {
            Session session;
            if (!this._currentSessions.TryPeek(out session))
            {
                throw new XRXPException("There is no session started. Please add a Session, before adding a Statistic.");
            }
            Statistic sessionStat = new Statistic(name, value, parameters, session);
            this.SendTrace(sessionStat);
        }

        private long GetTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public bool IsRecording()
        {
            return this._currentSessions.Count > 0;
        }

        public int RemainingTraceCount()
        {
            if (!_servicesStarted)
            {
                return 0;
            }
            int total = 0;
            foreach (IDataStorage storage in this._storages)
            {
                total += storage.RemainingDataCount();
            }
            return total;
        }

        public int PendingBackupFileCount()
        {
            return _backupResender != null ? _backupResender.GetPendingFileCount() : 0;
        }

        public void ResendBackups()
        {
            if (_backupResender == null)
            {
                Debug.LogWarning("XRXP.Recorder: Cannot resend backups — no remote storage configured.");
                return;
            }
            if (!_servicesStarted)
            {
                this.LaunchServices();
            }
            _backupResender.ResendAll();
        }

        public void Dispose()
        {
            if (!this._servicesStarted)
            {
                return;
            }
            if (this._currentSessions.Count > 0)
            {
                Debug.LogError(new XRXPException("A session is running. Please stop the sessions before stopping the XRXP record service."));
            }
            // Signal all storages to stop accepting, then wait for drain
            foreach (IDataStorage storage in this._storages)
            {
                storage.CompleteAdding();
            }
            foreach (IDataStorage storage in this._storages)
            {
                storage.Dispose();
            }
            this._servicesStarted = false;
        }
    }
}
