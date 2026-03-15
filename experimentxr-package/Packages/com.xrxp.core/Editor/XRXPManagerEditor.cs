using UnityEngine;
using UnityEditor;
using XRXP;
using XRXP.Recorder;

namespace XRXP.Editor
{
    [CustomEditor(typeof(XRXPManager))]
    public class XRXPManagerEditor : UnityEditor.Editor
    {
        SerializedProperty config;

        void OnEnable()
        {
            config = serializedObject.FindProperty(nameof(XRXPManager.config));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.ObjectField(config, typeof(XRXPConfig), new GUIContent("Configuration File"));
            serializedObject.ApplyModifiedProperties();
            
            GUILayout.Space(10);
            GUILayout.Label("Objects Tracked in the scene:", EditorStyles.boldLabel);
            
            var style = new GUIStyle();
            style.margin.left = 20;
            GUILayout.BeginVertical(style);
            
            XRXPObjectTracker[] objectTrackers = FindObjectsOfType<XRXPObjectTracker>();
            GUI.enabled = false;
            
            foreach (var objt in objectTrackers)
            {
                string frequency = (objt.TraceFrequency == 0) ? "1" : objt.TraceFrequency.ToString();
                GUILayout.Toggle(objt.TracingEnabled, $"{objt.GetObjectName()} - each {frequency} frames");
            }
            
            GUILayout.EndVertical();
            GUI.enabled = true;
            
            GUILayout.Space(6);
            if (GUILayout.Button("Refresh objects list"))
            {
                Repaint();
            }
        }
    }
}