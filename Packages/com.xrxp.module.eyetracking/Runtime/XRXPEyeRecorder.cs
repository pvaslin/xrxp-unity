using System;
using UnityEngine;
using XRXP;
using XRXP.Recorder;
using XRXP.Recorder.Models;

namespace XRXP.EyeTracking
{
    public class XRXPEyeRecorder : XRXPObjectTracker
    {
        public LineRenderer lefttmp;
        public LineRenderer righttmp;
        /// <summary>
        /// True if eye tracking is enabled, otherwise false.
        /// </summary>
        public bool EyeTrackingEnabled => OVRPlugin.eyeTrackingEnabled;

        /// <summary>
        /// A confidence value ranging from 0..1 indicating the reliability of the eye tracking data.
        /// </summary>
        public float Confidence { get; private set; }

        /// <summary>
        /// No record wil be done if detected eye state confidence is below this threshold.
        /// </summary>
        [Range(0f, 1f)]
        public float ConfidenceThreshold = 0.5f;

        private OVRPlugin.EyeGazesState _currentEyeGazesState;

        /// <summary>
        /// Reference frame for eye. If it's null, then world reference frame will be used.
        /// </summary>
        public Transform ReferenceFrame;

        private static int _trackingInstanceCount;

        private void OnEnable()
        {
            _trackingInstanceCount++;

            if (!this.StartEyeTracking())
            {
                base.enabled = false;
            }
        }

        private bool StartEyeTracking()
        {
            if (!OVRPlugin.StartEyeTracking())
            {
                Debug.LogWarning($"[{nameof(OVREyeGaze)}] Failed to start eye tracking.");
                return false;
            }

            return true;
        }

        private void OnDisable()
        {
            if (--_trackingInstanceCount == 0)
            {
                OVRPlugin.StopEyeTracking();
            }
        }

        OVRPose? GetEyePose(OVRPlugin.Eye eye)
        {
            var eyeGaze = _currentEyeGazesState.EyeGazes[(int)eye];

            if (!eyeGaze.IsValid)
                return null;

            Confidence = eyeGaze.Confidence;
            if (Confidence < ConfidenceThreshold)
                return null;

            var pose = eyeGaze.Pose.ToOVRPose();
            pose = pose.ToWorldSpacePose(Camera.main);
            return pose;
        }

        /// <summary>
        /// Call by the LateUpdate method in XRXPObjectTracker
        /// </summary>
        internal override void Record()
        {
            if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState))
                return;

            OVRPose? leftEyePose = this.GetEyePose(OVRPlugin.Eye.Left);
            OVRPose? rightEyePose = this.GetEyePose(OVRPlugin.Eye.Right);

            if (XRXPManager.IsReady && this.TracingEnabled && XRXPManager.Recorder.IsRecording() &&
            leftEyePose.HasValue && rightEyePose.HasValue)
            {
                DrawRayEye(this.lefttmp, leftEyePose.Value);
                DrawRayEye(this.righttmp, rightEyePose.Value);

                WorldPosition wpL = new WorldPosition(leftEyePose.Value.position, leftEyePose.Value.orientation);
                XRXPManager.Recorder.AddInternalEvent(SystemType.WorldPosition, "Eyes", "Left Eye", wpL);
                WorldPosition wpR = new WorldPosition(rightEyePose.Value.position, rightEyePose.Value.orientation);
                XRXPManager.Recorder.AddInternalEvent(SystemType.WorldPosition, "Eyes", "Right Eye", wpR);
            }
        }

        private void DrawRayEye(LineRenderer line, OVRPose pose)
        {
            var lookDirection = pose.orientation * Vector3.forward;
            line.SetPositions(new Vector3[] { pose.position, lookDirection * 100 });
            Debug.DrawRay(pose.position, lookDirection * 100, Color.blue, 1f);
        }

        public override string GetObjectName()
        {
            return "EyeRecorder";
        }

        public enum EyeTrackingMode
        {
            HeadSpace,
            WorldSpace,
            TrackingSpace
        }
    }
}