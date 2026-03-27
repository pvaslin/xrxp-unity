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
        private string _experimentId; // the ID of the experiment
        private User _user; // the current user
        private Stack<Session> _currentSessions;
        private bool _servicesStarted = false;
        private List<IDataStorage> _storages;
        private FileSender _fileSender = null;
        private string _workDirectory;
        private CancellationToken _cancellationToken;

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
            this._storages = new List<IDataStorage>();
            this._currentSessions = new Stack<Session>();
            this._cancellationToken = cancellationToken;
            if (backUpStorageMode)
            {
                this._storages.Add(new BackupStorage(this._workDirectory));
                // this._storages.Add(new BackupStorageSync(this._workDirectory));
            }
            if (webSocketServer != null)
            {
                this._storages.Add(new RemoteStorage(webSocketServer, authorizationToken));
            }
            if (fileServer != null)
            {
                this._fileSender = new FileSender(fileServer, authorizationToken);
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
            if (this._fileSender != null)
            {
                this._fileSender.Open(this._cancellationToken);
            }
        }

        private void SendTrace(RecordBase trace)
        {
            foreach (IDataStorage storage in this._storages)
            {
                storage.AsyncAdd(trace);
            }

            if (this._fileSender != null && trace is MediaEvent)
            {
                MediaEvent media = (trace as MediaEvent);
                this._fileSender.AsyncAdd(media.GetFilePath());
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
                // If the internalSystem doesn't exist create one
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
            Session session;
            if (!this._currentSessions.TryPeek(out session))
            {
                throw new XRXPException("There is no session started. Please add a Session, before adding a Question.");
            }
            Question question = new Question(label, answer, session);
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

            // Copy the file to the XRXP directory. This action use a Task to unload the MainThread
            Task.Run(() =>
            {
                Uri file = new Uri(filePath);
                if (file.IsFile && File.Exists(file.AbsolutePath))
                {
                    String mediaDirectory = $"{this._workDirectory}/sessions/{session.Id}/medias";
                    if (!Directory.Exists(mediaDirectory))
                    {
                        Directory.CreateDirectory(mediaDirectory);
                    }
                    string cpFilePath = $"{mediaDirectory}/{media.Id}{Path.GetExtension(file.AbsolutePath)}";
                    File.Copy(file.AbsolutePath, cpFilePath);

                    media.SetFilePath(cpFilePath);
                    this.SendTrace(media);
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
            this.SendTrace(media);

            // Save the binary to the XRXP directory. This action use a Task to unload the MainThread
            Task.Run(() =>
            {
                string mediaDirectory = $"{this._workDirectory}/sessions/{session.Id}/medias";
                if (!Directory.Exists(mediaDirectory))
                {
                    Directory.CreateDirectory(mediaDirectory);
                }
                string filePath = $"{mediaDirectory}/{media.Id}{MimeTypeMap.GetExtension(mimeType)}";
                try
                {
                    FileStream fs = File.Create(filePath);
                    fs.Write(bytes);
                    fs.Dispose();
                }
                catch (System.Exception)
                {

                    throw new XRXPException($"The file is not accessible ({filePath})");
                }
                media.SetFilePath(filePath);
                this.SendTrace(media);
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
                // Debug.Log($"Data to send Remaining for {storage.GetType().ToString()}: {storage.RemainingDataCount()}");
                total += storage.RemainingDataCount();
            }

            if (this._fileSender != null)
            {
                // Debug.Log($"Data to send Remaining for File: {this._fileSender.RemainingFileCount()}");
                total += this._fileSender.RemainingFileCount();
            }
            return total;
        }

        public async Task<bool> SafeDispose()
        {
            while (this.RemainingTraceCount() != 0)
            {
                // await Task.Yield(); 
                await Task.Delay(10);
            }
            this.Dispose();
            return true;
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
            foreach (IDataStorage storage in this._storages)
            {
                storage.Dispose();
            }

            if (this._fileSender != null)
            {
                this._fileSender.Dispose();
            }
            this._servicesStarted = false;
        }

        ~DataManager() // Finalizer
        {
            this.Dispose();
        }
    }
}