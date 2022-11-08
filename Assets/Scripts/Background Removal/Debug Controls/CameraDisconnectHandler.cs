using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;
using OpenCVForUnity.UnityUtils.Helper;
using TMPro;
using ArtScan.CoreModule;

namespace ArtScan.ErrorDisplayModule
{
    public class CameraDisconnectHandler : MonoBehaviour
    {
        public myWebCamTextureToMatHelper webCamTextureToMatHelper;
        public DebugMenu debugMenu;

        public Canvas errorDisplay;
        public TMP_Text errorText;
        public Canvas warningDisplay;
        public TMP_Text warningText;

        public bool doAttemptCameraRestart = true;

        public float autoRecoveryInterval = 10f;
        private bool doAttemptRecovery = false;

        public float timeout = 30f;

        public float lastUpdateTime;

        private void Awake()
        {
            if (webCamTextureToMatHelper == null)
                webCamTextureToMatHelper = (myWebCamTextureToMatHelper)FindObjectOfType(typeof(myWebCamTextureToMatHelper));

            if (debugMenu == null)
                debugMenu = (DebugMenu)FindObjectOfType(typeof(DebugMenu));

            errorDisplay.enabled = false;
            warningDisplay.enabled = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            lastUpdateTime = 0;
        }

        private void OnEnable()
        {
            if (webCamTextureToMatHelper != null)
            {
                webCamTextureToMatHelper.onInitialized.AddListener(ResetTimeoutCounter);
                webCamTextureToMatHelper.onInitialized.AddListener(HideCameraError);
                webCamTextureToMatHelper.onInitialized.AddListener(StopAutoRecovery);

                webCamTextureToMatHelper.onErrorOccurred.AddListener(ShowCameraError);
                webCamTextureToMatHelper.onErrorOccurred.AddListener(InitiateAutoRecovery);

                webCamTextureToMatHelper.onWarnOccurred.AddListener(ShowWarnDisplay);

                webCamTextureToMatHelper.onSuccessOccurred.AddListener(HideWarnDisplay);
            }
        }

        private void OnDisable()
        {
            if (webCamTextureToMatHelper != null)
            {
                webCamTextureToMatHelper.onInitialized.RemoveListener(ResetTimeoutCounter);
                webCamTextureToMatHelper.onInitialized.RemoveListener(HideCameraError);
                webCamTextureToMatHelper.onInitialized.RemoveListener(StopAutoRecovery);

                webCamTextureToMatHelper.onErrorOccurred.RemoveListener(ShowCameraError);
                webCamTextureToMatHelper.onErrorOccurred.RemoveListener(InitiateAutoRecovery);

                webCamTextureToMatHelper.onWarnOccurred.RemoveListener(ShowWarnDisplay);

                webCamTextureToMatHelper.onSuccessOccurred.RemoveListener(HideWarnDisplay);
            }
        }

        private void ResetTimeoutCounter()
        {
            lastUpdateTime = 0;
        }

        private void ShowCameraError(myWebCamTextureToMatHelper.ErrorCode errorCode)
        {
            errorDisplay.enabled = true;

            switch (errorCode)
            {
                case myWebCamTextureToMatHelper.ErrorCode.CAMERA_DEVICE_NOT_EXIST:
                    errorText.text = "No camera devices detected.";
                    break;

                case myWebCamTextureToMatHelper.ErrorCode.TIMEOUT:
                    errorText.text = "Camera has timed out.\n\nThis might be because the camera is not sending any frame data to the app.";
                    break;

                case myWebCamTextureToMatHelper.ErrorCode.CAMERA_PERMISSION_DENIED:
                    errorText.text = "Camera permission denied.";
                    break;
            }
        }

        private void HideCameraError()
        {
            if (errorDisplay.enabled)
            {
                RLMGLogger.Instance.Log(
                   System.String.Format("Camera {0} appears to be connected. Dismissing error display...", webCamTextureToMatHelper.requestedDeviceName),
                   MESSAGETYPE.INFO
               );
            }

            errorDisplay.enabled = false;
        }

        private void InitiateAutoRecovery(myWebCamTextureToMatHelper.ErrorCode errorCode)
        {
            StopAllCoroutines();

            doAttemptRecovery = true;
            StartCoroutine(AttemptAutoRecovery());
        }

        private void StopAutoRecovery()
        {
            doAttemptRecovery = false;
            StopAllCoroutines();
        }

        private IEnumerator AttemptAutoRecovery()
        {
            while (doAttemptRecovery)
            {
                ReInitialize();
                yield return new WaitForSeconds(autoRecoveryInterval);
            }
        }

        private void ShowWarnDisplay(myWebCamTextureToMatHelper.WarnCode warnCode)
        {
            warningDisplay.enabled = true;

            switch (warnCode)
            {
                case myWebCamTextureToMatHelper.WarnCode.WRONG_CAMERA_FRONTFACING_SELECTED:
                    warningText.text = "The requested camera was not found, and the first frontfacing camera was used instead.";
                    break;

                case myWebCamTextureToMatHelper.WarnCode.WRONG_CAMERA_FIRST_SELECTED:
                    warningText.text = "The requested camera was not found, and the first camera was used instead.";
                    break;
            }
        }

        private void HideWarnDisplay()
        {
            warningDisplay.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            CheckForDidUpdateFrame();
        }

        private void CheckForDidUpdateFrame()
        {
            if (webCamTextureToMatHelper != null)
            {
                if (webCamTextureToMatHelper.IsPlaying())
                {
                    if (!webCamTextureToMatHelper.DidUpdateThisFrame())
                    {
                        lastUpdateTime += Time.deltaTime;
                    }
                    else
                    {
                        lastUpdateTime = 0;
                    }

                    if (lastUpdateTime > timeout)
                    {
                        webCamTextureToMatHelper.Pause();

                        ReInitialize();
                    }
                }
            }
        }

        private void ReInitialize()
        {
            RLMGLogger.Instance.Log(
                System.String.Format("Re-initializing...", webCamTextureToMatHelper.requestedDeviceName),
                MESSAGETYPE.INFO
            );

            webCamTextureToMatHelper.Initialize();

            if (debugMenu != null)
                debugMenu.InitializeDebugMenu();
        }
    }
}


