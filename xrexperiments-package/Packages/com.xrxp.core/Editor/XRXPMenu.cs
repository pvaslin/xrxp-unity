using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using XRXP;

namespace XRXP.Editor
{
    public class XRXPMenu : MonoBehaviour
    {
        private const string ConfigFolderPath = "Assets/XRXPConfig";
        private const string ConfigAssetPath = "Assets/XRXPConfig/XRXPConfig.asset";
        private const string PackageIconPath = "Packages/com.xrxp.core/Editor/Icons/Tracker.png";
        
        [MenuItem("XRXP/Setup the scene", false, 10)]
        public static void SetupTheScene()
        {
            if (GameObject.Find("XRXP") != null)
            {
                Debug.Log("Scene already setup");
                return;
            }
            
            if (AssetDatabase.LoadAssetAtPath<XRXPConfig>(ConfigAssetPath) == null)
            {
                SetupConfig();
            }
            
            GameObject xrxp = new GameObject("XRXP");
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(PackageIconPath);
            if (icon != null)
            {
                EditorGUIUtility.SetIconForObject(xrxp, icon);
            }
            xrxp.AddComponent<XRXPManager>();
            xrxp.GetComponent<XRXPManager>().config = AssetDatabase.LoadAssetAtPath<XRXPConfig>(ConfigAssetPath);
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("XR Experiments (XRXP) scene setup complete!");
        }

        [MenuItem("XRXP/Add XRXP config", false, 10)]
        static void SetupConfig()
        {
            XRXPConfig config = ScriptableObject.CreateInstance<XRXPConfig>();
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(PackageIconPath);
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(ConfigFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "XRXPConfig");
            }
            
            AssetDatabase.CreateAsset(config, ConfigAssetPath);
            
            if (icon != null)
            {
                EditorGUIUtility.SetIconForObject(config, icon);
            }
            
            Debug.Log("Config has been created: " + AssetDatabase.GetAssetPath(config));

            if (GameObject.Find("XRXP") != null)
            {
                GameObject.Find("XRXP").GetComponent<XRXPManager>().config = AssetDatabase.LoadAssetAtPath<XRXPConfig>(ConfigAssetPath);
            }
        }

        [MenuItem("XRXP/User Guide")]
        static void OpenUserGuide()
        {
            Application.OpenURL("https://espace.science/xrxpdoc/");
        }

        [MenuItem("XRXP/About")]
        static void About()
        {
            Debug.Log("XR Experiments (XRXP) v1.0.0 - XR Experimentation Framework");
        }
    }
}