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
        private const string ModalityAssetPath = "Assets/XRXPConfig/ExchangeModality.asset";
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

            if (AssetDatabase.LoadAssetAtPath<ExchangeModality>(ModalityAssetPath) == null)
            {
                SetupExchangeModality();
            }

            GameObject xrxp = new GameObject("XRXP");
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(PackageIconPath);
            if (icon != null)
            {
                EditorGUIUtility.SetIconForObject(xrxp, icon);
            }

            // Add XRXPManager
            var manager = xrxp.AddComponent<XRXPManager>();
            manager.config = AssetDatabase.LoadAssetAtPath<XRXPConfig>(ConfigAssetPath);

            // Add XRXPExchangeManager
            var exchange = xrxp.AddComponent<XRXPExchangeManager>();
            exchange.Modality = AssetDatabase.LoadAssetAtPath<ExchangeModality>(ModalityAssetPath);
            exchange.Config = null; // Will use XRXPManager's config at runtime

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

            var xrxpGo = GameObject.Find("XRXP");
            if (xrxpGo != null)
            {
                var manager = xrxpGo.GetComponent<XRXPManager>();
                if (manager != null)
                {
                    manager.config = AssetDatabase.LoadAssetAtPath<XRXPConfig>(ConfigAssetPath);
                }
            }
        }

        [MenuItem("XRXP/Add Exchange Modality", false, 11)]
        static void SetupExchangeModality()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(ConfigFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "XRXPConfig");
            }

            ExchangeModality modality = ScriptableObject.CreateInstance<ExchangeModality>();
            AssetDatabase.CreateAsset(modality, ModalityAssetPath);

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(PackageIconPath);
            if (icon != null)
            {
                EditorGUIUtility.SetIconForObject(modality, icon);
            }

            Debug.Log("Exchange Modality has been created: " + AssetDatabase.GetAssetPath(modality));

            var xrxpGo = GameObject.Find("XRXP");
            if (xrxpGo != null)
            {
                var exchange = xrxpGo.GetComponent<XRXPExchangeManager>();
                if (exchange != null)
                {
                    exchange.Modality = AssetDatabase.LoadAssetAtPath<ExchangeModality>(ModalityAssetPath);
                }
            }
        }

        [MenuItem("XRXP/User Guide")]
        static void OpenUserGuide()
        {
            Application.OpenURL("https://xrxp.io/docs/");
        }

        [MenuItem("XRXP/About")]
        static void About()
        {
            Debug.Log("XR Experiments (XRXP) v1.0.0 - XR Experimentation Framework");
        }
    }
}