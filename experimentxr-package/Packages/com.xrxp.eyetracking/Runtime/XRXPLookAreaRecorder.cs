using System;
using UnityEngine;
using XRXP;
using XRXP.Recorder;
using XRXP.Recorder.Models;

namespace XRXP.EyeTracking
{
    public class XPXRLookAreaRecorder : MonoBehaviour
    {
        public const string DefaultLayerName = "XRXPArea";
        public bool TracingEnabled = true;
        /// <summary>
        /// A confidence value ranging from 0..1 indicating the reliability of the eye tracking data.
        /// </summary>
        public float Confidence { get; private set; }

        /// <summary>
        /// No record wil be done if detected eye state confidence is below this threshold.
        /// </summary>
        [Range(0f, 1f)]
        public float ConfidenceThreshold = 0.5f;
        public LayerMask AreaMask;
        private OVRPlugin.EyeGazesState _currentEyeGazesState;
        private Quaternion _initialRotationOffset;
        private Transform _viewTransform;
        private static int _trackingInstanceCount;
        private GameObject LastGMLookedByLeftEye;
        private GameObject LastGMLookedByRightEye;

        private void Start()
        {
            this.PrepareHeadDirection();
        }

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

        void LateUpdate()
        {
            if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState))
                return;

            OVRPose? leftEyePose = this.GetEyePose(OVRPlugin.Eye.Left);
            OVRPose? rightEyePose = this.GetEyePose(OVRPlugin.Eye.Right);

            if (XRXPManager.IsReady && this.TracingEnabled && XRXPManager.Recorder.isRecording() &&
            leftEyePose.HasValue && rightEyePose.HasValue)
            {
                GameObject gameObjectLookedLeft = this.GetGOLooked(leftEyePose.Value);
                GameObject gameObjectLookedRight = this.GetGOLooked(rightEyePose.Value);

                // Check if user looks the same things or not
                if (this.LastGMLookedByLeftEye == gameObjectLookedLeft && this.LastGMLookedByRightEye == gameObjectLookedRight)
                {
                    return;
                }
                
                this.LastGMLookedByLeftEye = gameObjectLookedLeft;
                this.LastGMLookedByRightEye = gameObjectLookedRight;

                // Actual record system
                if (gameObjectLookedLeft != null &&
                    gameObjectLookedRight != null &&
                    gameObjectLookedLeft == gameObjectLookedRight)
                {
                    Debug.Log($"User look only {gameObjectLookedLeft.name}");
                    XRXPManager.Recorder.AddLogEvent("User", "look", gameObjectLookedLeft.name);
                }
                else
                {
                    if (gameObjectLookedLeft != null)
                    {
                        Debug.Log($"User look left {gameObjectLookedLeft.name}");
                        XRXPManager.Recorder.AddLogEvent("User", "look", gameObjectLookedLeft.name);
                    }
                    if (gameObjectLookedRight != null)
                    {
                        Debug.Log($"User look right {gameObjectLookedRight.name}");
                        XRXPManager.Recorder.AddLogEvent("User", "look", gameObjectLookedRight.name);
                    }
                }
            }
        }

        private GameObject GetGOLooked(OVRPose pose)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(pose.position, pose.orientation * Vector3.forward, out raycastHit, 2f, this.AreaMask))
            {
                return raycastHit.transform.gameObject;
            }
            return null;
        }

        private void PrepareHeadDirection()
        {
            string transformName = "HeadLookAtDirection";

            this._viewTransform = new GameObject(transformName).transform;

            this._viewTransform.SetPositionAndRotation(base.transform.position, Quaternion.identity);

            this._viewTransform.parent = base.transform.parent;
            this._initialRotationOffset = Quaternion.Inverse(this._viewTransform.rotation) * base.transform.rotation;
        }

        public enum EyeTrackingMode
        {
            HeadSpace,
            WorldSpace,
            TrackingSpace
        }
    }
}