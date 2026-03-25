using UnityEngine;
using UnityEditor;
using XRXP.Recorder;

namespace XRXP.Editor
{
    [CustomEditor(typeof(XRXPObjectTracker))]
    public class XRXPObjectTrackerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            XRXPObjectTracker tracker = (XRXPObjectTracker)target;
            EditorGUI.BeginChangeCheck();
            
            bool tracingEnabled = EditorGUILayout.Toggle("Tracing Enabled", tracker.TracingEnabled);
            GUILayout.Space(10);
            
            string category = EditorGUILayout.TextField("Category", tracker.Category);
            EditorGUILayout.HelpBox("Type of object tracked (Body, Eye, Object etc)", MessageType.Info);
            GUILayout.Space(10);
            
            string objectName = EditorGUILayout.TextField("Object Name", tracker.ObjectName);
            EditorGUILayout.HelpBox("Name of the tracker (default: GameObject.Name)", MessageType.Info);
            GUILayout.Space(10);
            
            int traceFrequency = EditorGUILayout.IntSlider("Trace frequency", tracker.TraceFrequency, 0, 100);
            EditorGUILayout.HelpBox("For every x frames a record is made (0 = each frame)", MessageType.Info);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Edit XRXPObjectTracker");
                tracker.TracingEnabled = tracingEnabled;
                tracker.Category = category;
                tracker.ObjectName = !string.IsNullOrEmpty(objectName) ? objectName : tracker.gameObject.name;
                tracker.TraceFrequency = traceFrequency;

                PrefabUtility.RecordPrefabInstancePropertyModifications(tracker);
            }
        }
    }
}