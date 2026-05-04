using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XRXP;
using XRXP.EyeTracking;

namespace XRXP.EyeTracking.Editor
{
    public class XRXPEyeTrackingMenu : MonoBehaviour
    {
        [MenuItem("XRXP/Modules/Setup Eye Tracking Look Area", false, 10)]
        public static void SetupLookArea()
        {
            string newLayerName = XRXPLookAreaRecorder.DefaultLayerName;

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProperty = tagManager.FindProperty("layers");
            int layerAdded = LayerMask.NameToLayer(newLayerName);
            if (layerAdded == -1)
            {
                // Create the layer
                for (int i = 8; i < layersProperty.arraySize; i++)
                {
                    SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(i);
                    if (layerProperty.stringValue == "")
                    {
                        layerProperty.stringValue = newLayerName;
                        layerAdded = i;
                        break;
                    }
                }
            }

            if (layerAdded == -1)
            {
                throw new XRXPException("Failed to add layer. Maximum limit reached.");
            }

            tagManager.ApplyModifiedProperties();
            
            XRXPManager manager = Object.FindAnyObjectByType<XRXPManager>();
            if (manager == null)
            {
                Debug.LogError("XRXPManager not found in scene. Please run 'XRXP/Setup the scene' first.");
                return;
            }

            GameObject gm = manager.gameObject;
            
            if (gm.GetComponent<XRXPLookAreaRecorder>() == null)
            {
                XRXPLookAreaRecorder areaRecorder = gm.AddComponent<XRXPLookAreaRecorder>();
                areaRecorder.AreaMask = 1 << layerAdded;
            }
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("XRXP/Modules/Setup Eye Recorder", false, 11)]
        public static void SetupEyeRecorder()
        {
            XRXPManager manager = Object.FindAnyObjectByType<XRXPManager>();
            if (manager == null)
            {
                Debug.LogError("XRXPManager not found in scene. Please run 'XRXP/Setup the scene' first.");
                return;
            }

            GameObject gm = manager.gameObject;
            
            if (gm.GetComponent<XRXPEyeRecorder>() == null)
            {
                gm.AddComponent<XRXPEyeRecorder>();
            }
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
