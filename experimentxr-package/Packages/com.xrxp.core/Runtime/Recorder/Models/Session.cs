using System;
using System.Collections.Generic;

namespace XRXP.Recorder.Models
{
    public class Session : RecordBase
    {
        public string ExperimentId;
        public string UserId;
        public long StartDate; // UTC Unix time in ms
        public long EndDate; // UTC Unix time in ms
        public string Comments; // Use to describe the role of the user who joins the multi player Run
        public string EnvironmentId;
        public string Parent;
        private User _user;
        private Session _parent;
        private Environment _environment;
        private Dictionary<string, InternalSystem> _internalSystems; // the key is the name of the internal system

        public Session(string experimentId, User user, long startDate, String comments = null, Environment environment = null, Session parent = null)
            :this(Guid.NewGuid().ToString(), experimentId, user, startDate, comments, environment, parent, "") { }
        public Session(string experimentId, User user, long startDate, String comments = null,  Session parent = null, Environment environment = null)
            :this(Guid.NewGuid().ToString(), experimentId, user, startDate, comments, environment, parent, "") { }
        public Session(string id, string experimentId, User user, long startDate, String comments = null, Environment environment = null,  Session parent = null, string protocol = "")
        {
            this.Protocol = protocol.Length > 0 ? protocol : "Session";
            if (id == null)
            {
                this.Id = Guid.NewGuid().ToString();
            }
            else
            {
                this.Id = id;  
            }
            this.ExperimentId = experimentId;
            this._user = user;
            this.UserId = user.Id;
            this.StartDate = startDate;
            this.EndDate = 0;
            this.Comments = comments;
            this._internalSystems = new Dictionary<string, InternalSystem>();
            this._environment = environment;
            if (environment != null)
            {
                this.EnvironmentId = this._environment.Id;
            }
            this._parent = parent;
            if (parent != null)
            {
                this.Parent = parent.Id;
            }
        }

        public void AddInternalSystem(InternalSystem internalSystem)
        {
            this._internalSystems.Add(internalSystem.Name, internalSystem);
        }

        public bool TryGetInternalSystem(string name, out InternalSystem internalSystem)
        {
            if (this._internalSystems.ContainsKey(name))
            {
                internalSystem = this._internalSystems[name];
                return true;
            }
            internalSystem = null;
            return false;
        }

        public Environment GetEnvironment()
        {
            return this._environment;
        }

        public void UpdateEndDate(long endDate)
        {
            EndDate = endDate;
            this.Protocol = "Session_updateenddate";
            this.isSent = false;
        }

        public User GetUserInformation()
        {
            return this._user;
        }

    }
}
