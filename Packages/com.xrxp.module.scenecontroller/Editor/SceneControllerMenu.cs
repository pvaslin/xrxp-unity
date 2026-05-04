using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XRXP;
using XRXP.Modules.SceneController;

namespace XRXP.Modules.SceneController.Editor
{
    public class SceneControllerMenu : MonoBehaviour
    {
        [MenuItem("XRXP/Modules/Setup Scene Controller", false, 12)]
        public static void SetupSceneController()
        {
            XRXPManager manager = Object.FindAnyObjectByType<XRXPManager>();
            if (manager == null)
            {
                Debug.LogError("XRXPManager not found in scene. Please run 'XRXP/Setup the scene' first.");
                return;
            }

            GameObject gm = manager.gameObject;
            if (gm.GetComponent<XRXPSceneController>() == null)
            {
                gm.AddComponent<XRXPSceneController>();
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
