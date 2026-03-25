using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XRXP.Modules.SceneController;

namespace XRXP.Modules.SceneController.Editor
{
    public class SceneControllerMenu : MonoBehaviour
    {
        [MenuItem("XRXP/Modules/Setup Scene Controller", false, 12)]
        public static void SetupSceneController()
        {
            GameObject gm = GameObject.Find("XRXPManager");
            if (gm == null)
            {
                Debug.LogError("XRXPManager not found in scene. Please run 'XRXP/Setup the scene' first.");
                return;
            }

            if (gm.GetComponent<XRXPSceneController>() == null)
            {
                gm.AddComponent<XRXPSceneController>();
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
