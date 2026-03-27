using UnityEngine;

namespace XRXP
{
    /// <summary>
    /// Optional bridge between the XRXP Recorder and the Exchange system.
    /// Reports recorder status to the dashboard and allows triggering backup resend
    /// via the "xrxp.resendBackups" exchange control.
    ///
    /// Add this component to the same GameObject as XRXPExchangeManager.
    /// </summary>
    [RequireComponent(typeof(XRXPExchangeManager))]
    public class XRXPRecorderExchangeBridge : MonoBehaviour
    {
        private XRXPExchangeManager _exchange;
        private float _statusInterval = 2f;
        private float _nextStatusTime;

        private void Awake()
        {
            _exchange = GetComponent<XRXPExchangeManager>();
        }

        private void Update()
        {
            if (Time.time < _nextStatusTime || !XRXPManager.IsReady) return;

            _nextStatusTime = Time.time + _statusInterval;

            var recorder = XRXPManager.Recorder;
            _exchange.SetStatus("xrxp.pendingBackups", recorder.PendingBackupFileCount().ToString());
            _exchange.SetStatus("xrxp.transfersRemaining", recorder.TransfersState().ToString());
            _exchange.SetStatus("xrxp.isResending", recorder.IsResending.ToString().ToLower());
        }

        [ExchangeControl("xrxp.resendBackups")]
        private void OnResendBackups(string value)
        {
            if (!XRXPManager.IsReady)
            {
                Debug.LogWarning("XRXP.Exchange: Cannot resend backups — XRXPManager not ready.");
                return;
            }

            if (XRXPManager.Recorder.IsResending)
            {
                Debug.LogWarning("XRXP.Exchange: Resend already in progress.");
                return;
            }

            Debug.Log("XRXP.Exchange: Backup resend triggered from dashboard.");
            XRXPManager.Recorder.ResendBackups();
        }
    }
}
