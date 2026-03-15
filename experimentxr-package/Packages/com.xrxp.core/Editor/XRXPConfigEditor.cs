using UnityEngine;
using UnityEditor;
using XRXP;

namespace XRXP.Editor
{
    /// <summary>
    /// Custom editor for XRXPConfig ScriptableObject
    /// </summary>
    [CustomEditor(typeof(XRXPConfig))]
    public class XRXPConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            XRXPConfig targetConfig = (XRXPConfig)target;
            EditorGUI.BeginChangeCheck();
            
            GUILayout.Label("General", EditorStyles.boldLabel);
            string experimentID = EditorGUILayout.TextField("Experimentation ID", targetConfig.ExperimentID);
            
            GUILayout.Space(10);
            GUILayout.Label("Storage Modes", EditorStyles.boldLabel);
            bool onlineMode = EditorGUILayout.Toggle("Online mode", targetConfig.OnlineMode);
            
            GUI.enabled = false;
            bool backUpStorageMode = EditorGUILayout.Toggle("Backup mode", targetConfig.BackUpStorageMode);
            GUI.enabled = true;
            
            GUILayout.Space(10);
            GUILayout.Label("Server Configuration", EditorStyles.boldLabel);
            string webSocketServer = EditorGUILayout.TextField("WebSocket Server", targetConfig.WebSocketServer);
            string fileServer = EditorGUILayout.TextField("File Server", targetConfig.FileServer);
            string authorizationToken = EditorGUILayout.TextField("Authorization Token", targetConfig.AuthorizationToken);
            
            GUILayout.Space(10);
            GUILayout.Label("Coming Soon", EditorStyles.boldLabel);
            GUI.enabled = false;
            string restServer = EditorGUILayout.TextField("REST Server", targetConfig.RESTServer);
            bool localStorageMode = EditorGUILayout.Toggle("Local storage mode", targetConfig.LocalStorageMode);
            GUI.enabled = true;
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetConfig, "Modification of the XPXR Config");
                targetConfig.ExperimentID = experimentID;
                targetConfig.OnlineMode = onlineMode;
                targetConfig.BackUpStorageMode = backUpStorageMode;
                targetConfig.WebSocketServer = webSocketServer;
                targetConfig.FileServer = fileServer;
                targetConfig.AuthorizationToken = authorizationToken;
                targetConfig.RESTServer = restServer;
                targetConfig.LocalStorageMode = localStorageMode;
                
                EditorUtility.SetDirty(targetConfig);
            }
        }
    }
}