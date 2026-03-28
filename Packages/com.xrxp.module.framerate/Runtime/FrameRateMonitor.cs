using UnityEngine;

namespace XRXP.Modules.FrameRateAnalyser
{
    public class FrameRateMonitor : MonoBehaviour
    {
        public int Lag;
        public float Threshold;
        public float Influence;
        private PeakDetection _peakDetection;
        private float _deltaTime;

        void Start()
        {
            this._peakDetection = new PeakDetection(this.Lag, this.Threshold, this.Influence);
            this._deltaTime = 0;
        }

        void Update()
        {
            this._deltaTime += (Time.deltaTime - this._deltaTime) * 0.1f;
            int? isPeak = this._peakDetection.IsPeak(this._deltaTime);
#if UNITY_EDITOR
            FrameRateStats.PeakSignals.Value += (!isPeak.HasValue) ? 0 : isPeak.Value;
            FrameRateStats.FPSCounter.Value = (this._deltaTime > 0f) ? (1.0f / this._deltaTime) : 0f;
#endif
            
            if (!isPeak.HasValue || !XRXPManager.IsReady || !XRXPManager.Recorder.IsRecording())
            {
                return;
            }
            
            if (isPeak.Value == 1)
            {
                XRXPManager.Recorder.AddLogEvent("System", "detect", "a FPS spike");
            }
            else if (isPeak.Value == -1)
            {
                XRXPManager.Recorder.AddLogEvent("System", "detect", "a FPS drop");
            }
        }
    }
}
