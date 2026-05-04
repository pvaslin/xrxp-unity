using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XRXP;
using XRXP.Modules.FrameRateAnalyser;

namespace XRXP.Modules.FrameRateAnalyser.Editor
{
    public class FrameRateMenu : MonoBehaviour
    {
        [MenuItem("XRXP/Modules/Setup FrameRate Monitor", false, 13)]
        public static void SetupFrameRateMonitor()
        {
            XRXPManager manager = Object.FindAnyObjectByType<XRXPManager>();
            if (manager == null)
            {
                Debug.LogError("XRXPManager not found in scene. Please run 'XRXP/Setup the scene' first.");
                return;
            }

            GameObject gm = manager.gameObject;
            
            if (gm.GetComponent<FrameRateMonitor>() == null)
            {
                FrameRateMonitor monitor = gm.AddComponent<FrameRateMonitor>();
                // Set default values
                monitor.Lag = 30;
                monitor.Threshold = 5f;
                monitor.Influence = 0f;
            }
            
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
