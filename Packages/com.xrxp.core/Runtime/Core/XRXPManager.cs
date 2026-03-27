using System.Threading;
using UnityEngine;
using XRXP.Recorder;

namespace XRXP
{
    public class XRXPManager : MonoBehaviour
    {
        ////// Properties of control //////////////////////////////////
        private static XRXPManager _singleton = null; // Use for certify that there is only one XRXPManager
        private static XRXPRecorder _recorder;
        private static bool _isAwake = false; // Indicate if unity call the Awake method of the XRXPManager

        ////// Public Properties  /////////////////////////////////////
        public static XRXPRecorder Recorder
        {
            get
            {
                // Verification in case Trace is call in the Awake Unity Event for
                if (_recorder == null)
                {
                    if (_isAwake == false)
                    {
                        throw new XRXPException("Current Scene is not correctly setup for XR Experiments (XRXP). Resolution :\n- Please setup the scene in the menu XRXP > Setup the scene\n- Don't call XRXP in a Unity event Awake method.");
                    }
                    else
                    {
                        throw new XRXPException("XR Experiments (XRXP) is currently starting, verify the status with 'XRXPManager.IsReady'");
                    }
                }
                else
                {
                    return _recorder;
                }
            }
            private set { }
        }

        public static bool IsReady { get; private set; } = false;
        public XRXPConfig config;
        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            _isAwake = true;
            // Verify if the Experiment component is not already started
            if (_singleton == null)
            {
                _singleton = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Debug.LogWarning("XRXP : XR Experiments is already set in the scene");
                Destroy(this.gameObject);
                return;
            }

            // Start the init of Experiment

            // Launch of the Trace module
            this._cancellationTokenSource = new CancellationTokenSource();

#if UNITY_2022_2_OR_NEWER
            _recorder = new XRXPRecorder(this.config, base.destroyCancellationToken);
#else
            _recorder = new XRXPRecorder(this.config, this._cancellationTokenSource.Token);
#endif
            IsReady = true;
        }

        void OnApplicationQuit()
        {
            this._cancellationTokenSource.Cancel();
            _recorder.QuitApplication();
        }
    }
}