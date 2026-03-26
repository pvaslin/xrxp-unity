using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace XRXP.Modules.SceneController
{
    /// <summary>
    /// Scene controller that uses the Exchange system for bidirectional
    /// real-time scene management between the dashboard and Unity.
    ///
    /// Automatically registers a modality with:
    /// - Status field "currentScene" (streamed to dashboard)
    /// - Control field "changeScene" (dropdown with all build scenes)
    ///
    /// Uses [ExchangeControl("changeScene")] attribute for auto-discovery
    /// by XRXPExchangeManager — visible in the Inspector's attribute handlers section.
    ///
    /// Requires XRXPExchangeManager on the same or parent GameObject,
    /// or assign one via the ExchangeManager field.
    /// </summary>
    public class XRXPSceneController : MonoBehaviour
    {
        private static XRXPSceneController _singleton;

        [Header("Exchange")]
        [Tooltip("Reference to the XRXPExchangeManager. If null, will search on this GameObject and parents.")]
        public XRXPExchangeManager ExchangeManager;

        [Tooltip("Additional ExchangeModality to merge with the auto-generated scene modality. " +
                 "Use this to add custom status/control fields alongside scene management.")]
        public ExchangeModality AdditionalModality;

        [Header("Events")]
        public UnityEvent OnChangeScene;

        [Tooltip("Fired after a scene change with the new scene name")]
        public UnityEvent<string> OnSceneChanged;

        private List<string> _sceneNames;

        private void Awake()
        {
            if (_singleton == null)
            {
                _singleton = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Debug.LogWarning("XRXP: Scene Controller is already set in the scene");
                Destroy(this.gameObject);
                return;
            }
        }

        private void Start()
        {
            // Collect all scene names from build settings
            _sceneNames = new List<string>();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                int lastSlash = scenePath.LastIndexOf("/");
                _sceneNames.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1));
            }

            // Find or create the exchange manager
            if (ExchangeManager == null)
            {
                ExchangeManager = GetComponentInParent<XRXPExchangeManager>();
            }

            if (ExchangeManager == null)
            {
                ExchangeManager = gameObject.AddComponent<XRXPExchangeManager>();
            }

            // Build the scene exchange modality
            var modality = BuildSceneModality();
            ExchangeManager.Modality = modality;

            // Listen for scene changes to update status
            SceneManager.activeSceneChanged += ChangedActiveScene;

            // Set initial scene status once connected
            ExchangeManager.OnConnected.AddListener(OnExchangeConnected);
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= ChangedActiveScene;

            if (ExchangeManager != null)
            {
                ExchangeManager.OnConnected.RemoveListener(OnExchangeConnected);
            }
        }

        private ExchangeModality BuildSceneModality()
        {
            var modality = ScriptableObject.CreateInstance<ExchangeModality>();

            // Status: current scene
            modality.StatusFields = new List<ExchangeStatusField>
            {
                new ExchangeStatusField
                {
                    Key = "currentScene",
                    Label = "Current Scene",
                    Type = ExchangeFieldType.String,
                    DefaultValue = SceneManager.GetActiveScene().name
                }
            };

            // Control: change scene (dropdown with all build scenes)
            modality.ControlFields = new List<ExchangeControlField>
            {
                new ExchangeControlField
                {
                    Key = "changeScene",
                    Label = "Change Scene",
                    ControlType = ExchangeControlType.Dropdown,
                    DropdownOptions = _sceneNames.ToArray(),
                    DefaultValue = _sceneNames.Count > 0 ? _sceneNames[0] : ""
                }
            };

            // Merge additional modality fields if provided
            if (AdditionalModality != null)
            {
                modality.StatusFields.AddRange(AdditionalModality.StatusFields);
                modality.ControlFields.AddRange(AdditionalModality.ControlFields);
            }

            return modality;
        }

        private void OnExchangeConnected()
        {
            ExchangeManager.SetStatus("currentScene", SceneManager.GetActiveScene().name);
        }

        private void ChangedActiveScene(Scene current, Scene next)
        {
            if (ExchangeManager != null && ExchangeManager.IsConnected)
            {
                ExchangeManager.SetStatus("currentScene", next.name);
            }

            OnSceneChanged?.Invoke(next.name);
        }

        /// <summary>
        /// Handles the "changeScene" control from the dashboard.
        /// Auto-discovered by XRXPExchangeManager via [ExchangeControl] attribute.
        /// </summary>
        [ExchangeControl("changeScene")]
        public void HandleChangeScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("XRXP.SceneController: Received empty scene name");
                return;
            }

            if (_sceneNames.Contains(sceneName))
            {
                Debug.Log($"XRXP.SceneController: Changing scene to {sceneName}");
                OnChangeScene?.Invoke();
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"XRXP.SceneController: No scene by this name [{sceneName}]");
            }
        }
    }
}
