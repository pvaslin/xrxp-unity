using UnityEngine;

namespace XRXP
{
    [CreateAssetMenu(fileName = "XRXPConfig", menuName = "XRXP/XRXPConfig", order = 0)]
    public class XRXPConfig : ScriptableObject {
        public bool LocalStorageMode = false;
        public bool OnlineMode = true;
        public bool BackUpStorageMode = true;
        public string AuthorizationToken;
        public string WebSocketServer;
        public string RESTServer = "https://...";
        public string FileServer;
        public string ExperimentID = "Experimentation";
    }
}