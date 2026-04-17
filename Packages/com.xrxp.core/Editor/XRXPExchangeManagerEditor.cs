using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XRXP;

namespace XRXP.Editor
{
    [CustomEditor(typeof(XRXPExchangeManager))]
    public class XRXPExchangeManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _modality;
        private SerializedProperty _config;
        private SerializedProperty _controlBindings;
        private SerializedProperty _onControlReceived;
        private SerializedProperty _onConnected;
        private SerializedProperty _onDisconnected;

        private bool _showControlBindings = true;
        private bool _showAttributeHandlers = true;
        private bool _showStatusValues = true;
        private bool _showEvents = true;
        private List<(string key, string description)> _cachedEditModeHandlers;

        private void OnEnable()
        {
            _modality = serializedObject.FindProperty("Modality");
            _config = serializedObject.FindProperty("Config");
            _controlBindings = serializedObject.FindProperty("ControlBindings");
            _onControlReceived = serializedObject.FindProperty("OnControlReceived");
            _onConnected = serializedObject.FindProperty("OnConnected");
            _onDisconnected = serializedObject.FindProperty("OnDisconnected");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var manager = (XRXPExchangeManager)target;

            // ── Configuration ───────────────────────────────────────
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_modality);
            EditorGUILayout.PropertyField(_config);

            GUILayout.Space(6);

            // ── Sync bindings from Modality ─────────────────────────
            var modality = manager.Modality;
            if (modality != null)
            {
                SyncBindingsFromModality(manager, modality);
            }

            // ── Control Event Bindings ──────────────────────────────
            _showControlBindings = EditorGUILayout.Foldout(_showControlBindings, "Control Event Bindings", true, EditorStyles.foldoutHeader);
            if (_showControlBindings)
            {
                EditorGUI.indentLevel++;

                if (_controlBindings.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("No control fields defined in the Modality.", MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < _controlBindings.arraySize; i++)
                    {
                        var element = _controlBindings.GetArrayElementAtIndex(i);
                        var keyProp = element.FindPropertyRelative("Key");
                        var labelProp = element.FindPropertyRelative("Label");
                        var eventProp = element.FindPropertyRelative("OnReceived");

                        string displayName = !string.IsNullOrEmpty(labelProp.stringValue)
                            ? $"{labelProp.stringValue} ({keyProp.stringValue})"
                            : keyProp.stringValue;

                        EditorGUILayout.BeginVertical("box");

                        EditorGUILayout.LabelField(displayName, EditorStyles.miniLabel);

                        // Key is read-only
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(keyProp);
                        GUI.enabled = true;

                        // The UnityEvent is editable
                        EditorGUILayout.PropertyField(eventProp, new GUIContent("On Received (string)"));

                        EditorGUILayout.EndVertical();
                        GUILayout.Space(2);
                    }
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(4);

            // ── Attribute Handlers [ExchangeControl] ──────────────────
            _showAttributeHandlers = EditorGUILayout.Foldout(_showAttributeHandlers, "Attribute Handlers [ExchangeControl]", true, EditorStyles.foldoutHeader);
            if (_showAttributeHandlers)
            {
                EditorGUI.indentLevel++;

                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Rescan Scene"))
                    {
                        manager.DiscoverAttributeHandlers();
                    }

                    var handlers = manager.AttributeHandlers;
                    if (handlers == null || handlers.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No [ExchangeControl] handlers discovered. Click 'Rescan Scene' to search all MonoBehaviours.", MessageType.Info);
                    }
                    else
                    {
                        GUI.enabled = false;
                        foreach (var handler in handlers)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(handler.Key, GUILayout.Width(140));
                            EditorGUILayout.LabelField("\u2192", GUILayout.Width(20));
                            EditorGUILayout.LabelField(handler.TargetDescription);
                            EditorGUILayout.EndHorizontal();
                        }
                        GUI.enabled = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Scan Scene for [ExchangeControl]"))
                    {
                        _cachedEditModeHandlers = ScanAttributeHandlersEditMode();
                    }

                    var previewHandlers = _cachedEditModeHandlers ?? new List<(string, string)>();
                    if (previewHandlers.Count == 0)
                    {
                        EditorGUILayout.HelpBox("Click 'Scan Scene' to find [ExchangeControl] attributes on all MonoBehaviours in the scene.", MessageType.Info);
                    }
                    else
                    {
                        GUI.enabled = false;
                        foreach (var (key, desc) in previewHandlers)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(key, GUILayout.Width(140));
                            EditorGUILayout.LabelField("\u2192", GUILayout.Width(20));
                            EditorGUILayout.LabelField(desc);
                            EditorGUILayout.EndHorizontal();
                        }
                        GUI.enabled = true;
                    }
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(4);

            // ── Live Status Values (Play mode) ──────────────────────
            _showStatusValues = EditorGUILayout.Foldout(_showStatusValues, "Status Values (Live)", true, EditorStyles.foldoutHeader);
            if (_showStatusValues)
            {
                EditorGUI.indentLevel++;

                if (Application.isPlaying)
                {
                    var statusValues = manager.StatusValues;
                    if (statusValues == null || statusValues.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No status values available.", MessageType.Info);
                    }
                    else
                    {
                        GUI.enabled = false;
                        foreach (var kvp in statusValues)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(kvp.Key, GUILayout.Width(140));
                            EditorGUILayout.TextField(kvp.Value);
                            EditorGUILayout.EndHorizontal();
                        }
                        GUI.enabled = true;
                    }

                    // Auto-repaint in play mode for live updates
                    Repaint();
                }
                else
                {
                    if (modality != null && modality.StatusFields.Count > 0)
                    {
                        GUI.enabled = false;
                        foreach (var field in modality.StatusFields)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(field.Label ?? field.Key, GUILayout.Width(140));
                            EditorGUILayout.TextField(field.DefaultValue ?? "");
                            EditorGUILayout.EndHorizontal();
                        }
                        GUI.enabled = true;
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("No status fields defined. Assign a Modality to see fields.", MessageType.Info);
                    }
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(4);

            // ── Global Events ───────────────────────────────────────
            _showEvents = EditorGUILayout.Foldout(_showEvents, "Events", true, EditorStyles.foldoutHeader);
            if (_showEvents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_onControlReceived, new GUIContent("On Control Received (key, value)"));
                EditorGUILayout.PropertyField(_onConnected);
                EditorGUILayout.PropertyField(_onDisconnected);
                EditorGUI.indentLevel--;
            }

            // ── Connection Status (Play mode) ───────────────────────
            if (Application.isPlaying)
            {
                GUILayout.Space(6);
                EditorGUILayout.BeginHorizontal();
                var statusColor = manager.IsConnected ? Color.green : Color.red;
                var statusText = manager.IsConnected ? "Connected" : "Disconnected";
                var prevColor = GUI.color;
                GUI.color = statusColor;
                EditorGUILayout.LabelField("●", GUILayout.Width(16));
                GUI.color = prevColor;
                EditorGUILayout.LabelField($"WebSocket: {statusText}");
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Syncs the serialized ControlBindings list to match the Modality's control fields.
        /// Adds missing keys, removes stale ones, preserves existing event wiring.
        /// </summary>
        private void SyncBindingsFromModality(XRXPExchangeManager manager, ExchangeModality modality)
        {
            var bindings = manager.ControlBindings;
            var existingKeys = new HashSet<string>();
            foreach (var b in bindings) existingKeys.Add(b.Key);

            var modalityKeys = new HashSet<string>();
            bool changed = false;

            // Add missing bindings
            foreach (var field in modality.ControlFields)
            {
                modalityKeys.Add(field.Key);

                if (!existingKeys.Contains(field.Key))
                {
                    bindings.Add(new ExchangeEventBinding
                    {
                        Key = field.Key,
                        Label = field.Label,
                        OnReceived = new UnityEngine.Events.UnityEvent<string>()
                    });
                    changed = true;
                }
                else
                {
                    // Update label if changed
                    var existing = bindings.Find(b => b.Key == field.Key);
                    if (existing != null && existing.Label != field.Label)
                    {
                        existing.Label = field.Label;
                        changed = true;
                    }
                }
            }

            // Remove stale bindings (keys no longer in modality)
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                if (!modalityKeys.Contains(bindings[i].Key))
                {
                    bindings.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(manager);
            }
        }

        /// <summary>
        /// Scans for [ExchangeControl] attributes in edit mode across all MonoBehaviours in the scene.
        /// </summary>
        private List<(string key, string description)> ScanAttributeHandlersEditMode()
        {
            var results = new List<(string, string)>();
            var components = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (var component in components)
            {
                if (component == null) continue;

                var type = component.GetType();
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var method in methods)
                {
                    var attrs = method.GetCustomAttributes(typeof(ExchangeControlAttribute), true);
                    foreach (ExchangeControlAttribute attr in attrs)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                        {
                            results.Add((attr.Key, $"{type.Name}.{method.Name}(string)"));
                        }
                    }
                }
            }

            return results;
        }
    }
}
