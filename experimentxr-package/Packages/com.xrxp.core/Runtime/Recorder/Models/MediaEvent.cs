
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using MimeTypes;

namespace XRXP.Recorder.Models
{
    public class MediaEvent : RecordBase
    {
        public long Time;
        public int Duration;
        public string MimeType;
        public string Name;
        public string SessionId;
        public string UserId;
        private Session _session;
        private User _user;
        private string _directory;
        private string _filePath;

        public MediaEvent(long time, string mimeType, string name, User user, Session session, int duration = 0, string protocol = "")
        {
            this.Id = Guid.NewGuid().ToString();
            this.Time = time;
            this.MimeType = mimeType;
            this.Name = name;
            this._user = user;
            this.UserId = user.Id;
            this._session = session;
            this.SessionId = session.Id;
            this.Duration = duration;
            this.Protocol = protocol.Length > 0 ? protocol : "MediaEvent";
        }

        public void SetFilePath(string filepath)
        {
            this._filePath = filepath;
        }
        public string GetFilePath()
        {
            return this._filePath;
        }
    }
}

